using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class MappingEmitContext
    {
        private KnownMapper _mapper;

        private readonly EmitContext _emitContext;

        private readonly HashSet<IMethodSymbol> _mappingMethods = new(SymbolEqualityComparer.Default);

        private readonly HashSet<IMethodSymbol> _converterMethods = new(SymbolEqualityComparer.Default);

        private readonly HashSet<IPropertySymbol> _sourceProperties = new(SymbolEqualityComparer.Default);

        private readonly Dictionary<string, CustomizedMapping> _customizedMappings = new(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<IPropertySymbol> _destinationCandidateProperties = new(SymbolEqualityComparer.Default);

        private readonly HashSet<MappingDefinition> _destinationProperties = new(MappingDefinitionEqualityComparer.IgnoreCase);

        public INamedTypeSymbol MapperType => _emitContext.MapperType;

        public IGeneratorContext ExecutionContext => _emitContext.ExecutionContext;

        public KnownTypeSymbols KnownTypes => _emitContext.KnownTypes;

        public CollectionClassifier CollectionClassifier => _emitContext.CollectionClassifier;

        public MemberNamingManager MemberNamingManager => _emitContext.MemberNamingManager;

        public IReadOnlyCollection<KnownMapperRef> MemberMappers => _emitContext.MemberMappers;
        
        public INamedTypeSymbol SourceType { get; private set; } = default!;

        public INamedTypeSymbol DestinationType { get; private set; } = default!;

        public IReadOnlyCollection<KnownMapper> InternalMappers { get; private set; } = default!;

        public IReadOnlyCollection<KnownMapper> KnownMappers { get; private set; } = default!;

        public MissingMappingBehavior MissingMappingBehaviour => _mapper.MissingMappingBehavior;

        public ImplementationType ImplementationType => _mapper.ImplementationType;

        public ConstructorAccessibility ConstructorAccessibility => _mapper.ConstructorAccessibility;

        private string MapperName => _mapper.LocalName ?? string.Empty;

        public IReadOnlyCollection<IMethodSymbol> MappingMethods => _mappingMethods;

        public IReadOnlyCollection<IPropertySymbol> SourceProperties => _sourceProperties;

        public ImmutableList<MappingDefinition> DestinationProperties => _destinationProperties.ToImmutableList();

        public string AfterMapMethodName => $"{MapperName}AfterMap";

        public string DestinationConstructorName => $"{MapperName}CreateDestination";

        public string MapMethodName(string suffix) => $"{MapperName}Map{suffix}";

        private MappingEmitContext(KnownMapper mapper, EmitContext emitContext)
        {
            _emitContext = emitContext;
            _mapper = mapper;
        }

        public MappingDefinition MakeMappingDefinition(IPropertySymbol symbol)
        {
            _customizedMappings.TryGetValue(symbol.Name, out var cm);

            return new MappingDefinition(symbol.Name, symbol.Type, symbol, cm);
        }

        public MappingDefinition MakeMappingDefinition(IParameterSymbol symbol)
        {
            _customizedMappings.TryGetValue(symbol.Name, out var cm);

            return new MappingDefinition(symbol.Name, symbol.Type, symbol, cm);
        }

        public void DestinationMapped(MappingDefinition definition)
        {
            _destinationProperties.Remove(definition);
        }

        public IMethodSymbol? TryGetTypeConverter(ITypeSymbol source, ITypeSymbol destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            foreach (var converter in _converterMethods)
            {
                var inputType = converter.Parameters[0].Type;
                var returnType = converter.ReturnType;

                var inputConv = ExecutionContext.Compilation.ClassifyConversion(source, inputType);
                var returnConv = ExecutionContext.Compilation.ClassifyConversion(returnType, destination);

                if (inputConv.Exists && inputConv.IsImplicit && returnConv.Exists && returnConv.IsImplicit)
                    return converter;
            }

            return null;
        }

        public MapperInstanceSyntaxModel CreateSyntaxModel()
        {
            return new MapperInstanceSyntaxModel(
                ExecutionContext,
                MapperName,
                MapperType.ContainingNamespace,
                MapperType,
                SourceType,
                DestinationType,
                DestinationConstructorName,
                ImplementationType,
                KnownTypes
                );
        }

        public static MappingEmitContext Build(
            EmitContext emitContext,
            KnownMapper mapperType, 
            INamedTypeSymbol sourceType,
            INamedTypeSymbol destinationType)
        {
            if (emitContext == null)
                throw new ArgumentNullException(nameof(emitContext));
            
            var result = new MappingEmitContext(mapperType, emitContext);

            result.SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            result.DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));

            SetInternalKnownMappers(result, emitContext.InternalMappers);
            SetKnownMappers(result, emitContext.KnownMappers);
            SetCustomConverterMethods(result);
            SetSourceProperties(result);
            SetDestinationCandidateProperties(result);
            SetCustomizedMappings(result);
            SetDestinationProperties(result);
            SetCustomMappingMethods(result);

            return result;
        }

        private static void SetDestinationProperties(MappingEmitContext result)
        {
            foreach (var prop in result._destinationCandidateProperties)
            {
                if (!result.ExecutionContext.Compilation.IsSymbolAccessibleWithin(prop, result.MapperType))
                    continue;

                var colType = result.CollectionClassifier.ClassifyCollectionType(prop.Type);

                if (!colType.IsEnumerable)
                {
                    if (prop.IsReadOnly)
                        continue;

                    if (prop.SetMethod == null)
                        continue;

                    if (!result.ExecutionContext.Compilation.IsSymbolAccessibleWithin(prop.SetMethod, result.MapperType))
                        continue;
                }

                var entry = result.MakeMappingDefinition(prop);
                result._destinationProperties.Add(entry);
            }
        }

        private static void SetInternalKnownMappers(MappingEmitContext context, IEnumerable<KnownMapper> knownMappers)
        {
            var km = knownMappers.Where(
                p => !(p.SourceType.Equals(context.SourceType, SymbolEqualityComparer.Default) && p.DestType.Equals(context.DestinationType, SymbolEqualityComparer.Default))
                );

            context.InternalMappers = new List<KnownMapper>(km);
        }

        private static void SetKnownMappers(MappingEmitContext context, IEnumerable<KnownMapper> knownMappers)
        {
            var knownMappersToIgnore = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var knownMappersToIgnoreAttr = context.MapperType.GetAttributes().Where(
                p => string.Equals(p.AttributeClass?.ToDisplayString(), typeof(MappingGeneratorMappersIgnoreAttribute).FullName)
                );

            foreach (var args in knownMappersToIgnoreAttr.SelectMany(static p => p.ConstructorArguments))
            {
                var values = args.Values.Select(p => (INamedTypeSymbol?)p.Value).Where(p => p != null);

                foreach (var val in values)
                    knownMappersToIgnore.Add(val!);
            }

            var km = knownMappers
                .Where(p => !p.Mapper.Equals(context.MapperType, SymbolEqualityComparer.Default))
                .Where(p => !knownMappersToIgnore.Contains(p.Mapper));

            context.KnownMappers = new List<KnownMapper>(km);
        }

        private static void SetCustomMappingMethods(MappingEmitContext context)
        {
            var prefix = $"{context.MapperName}Map";

            var mappingMethods = context.MapperType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(p => p.Name.StartsWith(prefix))
                .ToList();

            foreach (var mm in mappingMethods)
            {
                if (mm.Parameters.Count() != 1)
                {
                    context.ExecutionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.BadMappingMethodSignature,
                            mm.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            context.SourceType.ToDisplayString()
                            )
                        );
                    continue;
                }

                var isVoid = mm.ReturnType.Equals(
                    context.ExecutionContext.Compilation.GetSpecialType(SpecialType.System_Void), SymbolEqualityComparer.Default
                    );

                if (isVoid)
                {
                    context.ExecutionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.BadMappingMethodSignature,
                            mm.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            context.SourceType.ToDisplayString()
                            )
                        );
                    continue;
                }

                if (!mm.Parameters.First().Type.Equals(context.SourceType, SymbolEqualityComparer.Default))
                {
                    context.ExecutionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.BadMappingMethodSignature,
                            mm.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            context.SourceType.ToDisplayString()
                            )
                        );
                    continue;
                }

                var propName = mm.Name.Substring(prefix.Length);
                var hasMatch = context._destinationProperties.Any(p => string.Equals(p.Name, propName, StringComparison.Ordinal));

                if (!hasMatch)
                {
                    context.ExecutionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.MappingMethodHasNoDestination,
                            mm.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            mm.ToDisplayString(),
                            context.DestinationType.ToDisplayString(),
                            propName
                            )
                        );
                    continue;
                }

                context._mappingMethods.Add(mm);
            }
        }
        
        private static void SetCustomConverterMethods(MappingEmitContext context)
        {
            var mappingMethods = context.MapperType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(p => p.Name.StartsWith($"{context.MapperName}Convert"))
                .ToList();

            foreach (var mm in mappingMethods)
            {
                if (mm.Parameters.Count() != 1)
                {
                    context.ExecutionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.BadMappingMethodSignature,
                            mm.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            context.SourceType.ToDisplayString()
                            )
                        );
                    continue;
                }

                var isVoid = mm.ReturnType.Equals(
                    context.ExecutionContext.Compilation.GetSpecialType(SpecialType.System_Void), 
                    SymbolEqualityComparer.Default
                    );

                if (isVoid)
                {
                    context.ExecutionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.BadMappingMethodSignature,
                            mm.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            context.SourceType.ToDisplayString()
                            )
                        );
                    continue;
                }

                context._converterMethods.Add(mm);
            }
        }

        private static void SetSourceProperties(MappingEmitContext context)
        {
            void GetSourceMembers(INamedTypeSymbol? type)
            {
                if (type == null)
                    return;

                var sourceProperties = type.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p => !p.IsStatic)
                    .Where(p => p.GetMethod != null)
                    .Where(p => context.ExecutionContext.Compilation.IsSymbolAccessibleWithin(p.GetMethod!, context.MapperType));

                foreach (var sp in sourceProperties)
                    context._sourceProperties.Add(sp);

                GetSourceMembers(type.BaseType);
            }

            GetSourceMembers(context.SourceType);
        }

        private static void SetDestinationCandidateProperties(MappingEmitContext context)
        {
            var ignoreAttr = context.MapperType.GetAttributes()
                .Where(
                    p => string.Equals(
                        p.AttributeClass?.ToDisplayString(),
                        typeof(MappingGeneratorPropertyIgnoreAttribute).FullName,
                        StringComparison.Ordinal
                        )
                    )
                .ToList();

            var propertiesToIgnore = new HashSet<string>();

            foreach (var args in ignoreAttr.SelectMany(p => p.ConstructorArguments))
            {
                var values = args.Values.Select(p => (string?)p.Value).Where(p => p != null);

                foreach (var val in values)
                    propertiesToIgnore.Add(val!);
            }

            void GetDestMembers(INamedTypeSymbol? type)
            {
                if (type == null)
                    return;

                var sourceProperties = type.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p => !p.IsStatic)
                    .Where(p => !propertiesToIgnore.Contains(p.Name));

                foreach (var sp in sourceProperties)
                    context._destinationCandidateProperties.Add(sp);

                GetDestMembers(type.BaseType);
            }

            GetDestMembers(context.DestinationType);
        }

        private static void SetCustomizedMappings(MappingEmitContext context)
        {
            var customAttr = context.MapperType.GetAttributes()
                .Where(
                    p => string.Equals(
                        p.AttributeClass?.ToDisplayString(),
                        typeof(MappingGeneratorPropertyMappingAttribute).FullName,
                        StringComparison.Ordinal
                        )
                    )
                .ToList();

            foreach (var customMapping in customAttr)
            {
                var sourceName = (string)customMapping.ConstructorArguments[0].Value!;
                var destName = (string)customMapping.ConstructorArguments[1].Value!;

                var source = context._sourceProperties.FirstOrDefault(p => string.Equals(p.Name, sourceName, StringComparison.Ordinal));

                if (source == null)
                {
                    context.ExecutionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidProperty,
                            context.MapperType.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            "Source",
                            context.SourceType.ToDisplayString(),
                            sourceName
                            )
                        );
                }

                var dest = context._destinationCandidateProperties.FirstOrDefault(
                    p => string.Equals(p.Name, destName, StringComparison.Ordinal)
                    );

                if (dest == null)
                {
                    context.ExecutionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidProperty,
                            context.MapperType.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            "Destination",
                            context.DestinationType.ToDisplayString(),
                            destName
                            )
                        );
                }

                if (source == null || dest == null)
                    throw new MappingGenerationException("Bad configuration");

                context._customizedMappings[dest.Name] = new CustomizedMapping(source);
            }
        }
    }
}
