﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using MappingGenerator.Abstractions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MappingGenerator.SourceGeneration
{
    internal class MappingGenerationContext
    {
        private readonly HashSet<IMethodSymbol> _mappingMethods = new(SymbolEqualityComparer.Default);

        private readonly HashSet<IMethodSymbol> _converterMethods = new(SymbolEqualityComparer.Default);

        private readonly HashSet<IPropertySymbol> _sourceProperties = new(SymbolEqualityComparer.Default);

        private readonly Dictionary<string, CustomizedMapping> _customizedMappings = new(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<IPropertySymbol> _destinationCandidateProperties = new(SymbolEqualityComparer.Default);

        private readonly HashSet<MappingDefinition> _destinationProperties = new(MappingEntryEqualityComparer.IgnoreCase);

        public INamedTypeSymbol MapperType { get; private set; } = default!;

        public INamedTypeSymbol SourceType { get; private set; } = default!;

        public INamedTypeSymbol DestinationType { get; private set; } = default!;

        public IReadOnlyCollection<KnownMapper> KnownMappers { get; private set; } = default!;

        public IMappingSourceGeneratorContext ExecutionContext { get; }

        public MissingMappingBehavior MissingMappingBehaviour { get; private set; }

        public ImplementationType ImplementationType { get; private set; }

        public ConstructorAccessibility ConstructorAccessibility { get; private set; }

        public string MapperName { get; private set; } = default!;

        public IReadOnlyCollection<IMethodSymbol> MappingMethods => _mappingMethods;

        public IReadOnlyCollection<IPropertySymbol> SourceProperties => _sourceProperties;

        public ImmutableList<MappingDefinition> DestinationProperties => _destinationProperties.ToImmutableList();

        public string DestinationConstructorMethodName => $"{MapperName}CreateDestination";

        public string AfterMapMethodName => $"{MapperName}AfterMap";

        public string MapMethodName(string suffix) => $"{MapperName}Map{suffix}";

        public KnownTypeSymbols KnownTypes { get; }

        public CollectionClassifier CollectionClassifier { get; }

        private MappingGenerationContext(IMappingSourceGeneratorContext executionContext)
        {
            ExecutionContext = executionContext;
            KnownTypes = new KnownTypeSymbols(executionContext.Compilation);
            CollectionClassifier = new CollectionClassifier(KnownTypes);
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

        public MappingSyntaxModel CreateSyntaxModel()
        {
            return new MappingSyntaxModel(
                this,
                MapperType.ContainingNamespace,
                MapperType,
                SourceType,
                DestinationType
                );
        }

        public static MappingGenerationContext Build(
            INamedTypeSymbol mapperType, 
            INamedTypeSymbol sourceType,
            INamedTypeSymbol destinationType,
            IEnumerable<KnownMapper> knownMappers,
            IMappingSourceGeneratorContext executionContext)
        {
            if (knownMappers == null)
                throw new ArgumentNullException(nameof(knownMappers));
            
            var result = new MappingGenerationContext(executionContext);

            result.MapperType = mapperType ?? throw new ArgumentNullException(nameof(mapperType));
            result.SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            result.DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));

            var mappingAttr = mapperType.GetAttributes().FirstOrDefault(
                p => string.Equals(p.AttributeClass?.ToDisplayString(), typeof(MappingGeneratorAttribute).FullName)
                );

            if (mappingAttr == null)
            {
                throw new MappingGenerationException(
                    $"Mapper type '{mapperType.ToDisplayString()}' does not have '{typeof(MappingGeneratorAttribute).FullName}' specified"
                    );
            }

            foreach (var mapperNamedArg in mappingAttr.NamedArguments)
            {
                if (string.Equals(mapperNamedArg.Key, nameof(MappingGeneratorAttribute.MissingMappingBehavior), StringComparison.Ordinal))
                    result.MissingMappingBehaviour = (MissingMappingBehavior)((int?)mapperNamedArg.Value.Value ?? 0);

                if (string.Equals(mapperNamedArg.Key, nameof(MappingGeneratorAttribute.Name), StringComparison.Ordinal))
                    result.MapperName = (string?)mapperNamedArg.Value.Value ?? string.Empty;

                if (string.Equals(mapperNamedArg.Key, nameof(MappingGeneratorAttribute.ImplementationType), StringComparison.Ordinal))
                    result.ImplementationType = (ImplementationType)((int?)mapperNamedArg.Value.Value ?? 0);

                if (string.Equals(mapperNamedArg.Key, nameof(MappingGeneratorAttribute.ConstructorAccessibility), StringComparison.Ordinal))
                    result.ConstructorAccessibility = (ConstructorAccessibility)((int?)mapperNamedArg.Value.Value ?? 0);
            }

            SetKnownMappers(result, knownMappers);
            SetCustomMappingMethods(result);
            SetCustomConvertorMethods(result);
            SetSourceProperties(result);
            SetDestinationCandidateProperties(result);
            SetCustomizedMappings(result);
            SetDestinationProperties(result);

            return result;
        }

        private static void SetDestinationProperties(MappingGenerationContext result)
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

        private static void SetKnownMappers(MappingGenerationContext context, IEnumerable<KnownMapper> knownMappers)
        {
            var knownMappersToIgnore = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var knownMappersToIgnoreAttr = context.MapperType.GetAttributes().Where(
                p => string.Equals(p.AttributeClass?.ToDisplayString(), typeof(MappingGeneratorMappersIgnoreAttribute).FullName)
                );

            foreach (var args in knownMappersToIgnoreAttr.SelectMany(p => p.ConstructorArguments))
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

        private static void SetCustomMappingMethods(MappingGenerationContext context)
        {
            var mappingMethods = context.MapperType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(p => p.Name.StartsWith($"{context.MapperName}Map"))
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

                context._mappingMethods.Add(mm);
            }
        }
        
        private static void SetCustomConvertorMethods(MappingGenerationContext context)
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

        private static void SetSourceProperties(MappingGenerationContext context)
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

        private static void SetDestinationCandidateProperties(MappingGenerationContext context)
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

        private static void SetCustomizedMappings(MappingGenerationContext context)
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
                            context.SourceType.Locations.FirstOrDefault(),
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
                            context.DestinationType.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            "Destination",
                            context.DestinationType.ToDisplayString(),
                            destName
                            )
                        );
                }

                if (source != null && dest != null)
                {
                    context._customizedMappings[dest.Name] = new CustomizedMapping(source);
                }
            }
        }
    }
}
