using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{

    [Generator]
    public class MappingSourceGenerator : IIncrementalGenerator
    {
        private static readonly string MappingGeneratorAttributeName = typeof(MappingGeneratorAttribute).FullName;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode) =>
                syntaxNode is ClassDeclarationSyntax c && c.AttributeLists.Count > 0;

            IncrementalValuesProvider<ClassDeclarationSyntax> mapperDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (p, _) => IsSyntaxTargetForGeneration(p),
                    static (p, _) => GetSemanticTargetForGeneration(p))
                .Where(static m => m != null)!;

            var compilationAndClasses = context.CompilationProvider.Combine(mapperDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            foreach (var attrList in classDeclaration.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    if (ModelExtensions.GetSymbolInfo(context.SemanticModel, attr).Symbol is not IMethodSymbol attrSymbol)
                        continue;

                    var attrName = attrSymbol.ContainingType.ToDisplayString();

                    if (string.Equals(attrName, MappingGeneratorAttributeName, StringComparison.Ordinal))
                        return classDeclaration;
                }
            }

            return null;
        }

        private static void Execute(
            Compilation compilation, 
            ImmutableArray<ClassDeclarationSyntax> classes, 
            SourceProductionContext context)
        {
            var knownMappers = new Dictionary<INamedTypeSymbol, List<KnownMapper>>(SymbolEqualityComparer.Default);

            foreach (var mapper in classes)
            {
                var semanticModel = compilation.GetSemanticModel(mapper.SyntaxTree);
                var model = ModelExtensions.GetDeclaredSymbol(semanticModel, mapper) as INamedTypeSymbol;

                if (model == null)
                    return;

                var mappingAttributes = model.GetAttributes().Where(
                    p => string.Equals(p.AttributeClass?.ToDisplayString(), MappingGeneratorAttributeName, StringComparison.Ordinal)
                    ).ToList();

                if (mappingAttributes.Count == 0)
                    continue;

                if (model.ContainingType != null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.CantBeNestedClass,
                            mapper.GetLocation(),
                            model.ToDisplayString()
                            )
                        );

                    continue;
                }

                if (!mapper.Modifiers.Any(p => p.IsKind(SyntaxKind.PartialKeyword)))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.NotPartial,
                            mapper.GetLocation(),
                            model.ToDisplayString()
                            )
                        );

                    continue;
                }

                foreach (var attr in mappingAttributes)
                {
                    if (TryBuildKnownMapper(model, attr, context, out var knownMapper))
                    {
                        if (!knownMappers.ContainsKey(model))
                            knownMappers.Add(model, new List<KnownMapper>());

                        if (knownMapper == null)
                            continue;

                        if (!IsValidMappingGenerator(model, knownMappers[model], knownMapper, context))
                            continue;

                        knownMappers[model].Add(knownMapper);
                    }
                }
            }

            var allKnownMappers = knownMappers.Values.SelectMany(p => p);

            foreach (var mapper in knownMappers)
            {
                var generator = new Generator(mapper.Key, mapper.Value, allKnownMappers);
                var generatorContext = new GeneratorContext(compilation, context.ReportDiagnostic);

                try
                {
                    var result = generator.Build(generatorContext);
                    var source = new StringBuilder();

                    foreach (var syntax in result)
                    {
                        source.Append(syntax.GetText(Encoding.UTF8));
                        source.AppendLine();
                    }
                            
                    context.AddSource($"{generator.FileName}.g.cs", source.ToString());
                }
                catch (MappingGenerationException)
                {
                    // context.ReportDiagnostic()
                }
            }
        }

        private static bool IsValidMappingGenerator(
            INamedTypeSymbol model,
            IReadOnlyCollection<KnownMapper> mappers, 
            KnownMapper mapper,
            SourceProductionContext context)
        {
            if (mappers.Count == 0)
                return true;

            if (mapper.LocalName != null)
            {
                if (mappers.Any(p => string.Equals(p.LocalName, mapper.LocalName, StringComparison.Ordinal)))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.MapperNameDuplicate,
                            model.Locations.FirstOrDefault(),
                            model.ToDisplayString(),
                            mapper.LocalName
                            )
                        );
                    return false;
                }
            }

            var sameMapper = mappers
                .FirstOrDefault(
                    p => p.SourceType.Equals(mapper.SourceType, SymbolEqualityComparer.Default)
                        && p.DestType.Equals(mapper.DestType, SymbolEqualityComparer.Default)
                    );

            if (sameMapper != null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.MapperConflictSameMapper,
                        model.Locations.FirstOrDefault(),
                        model.ToDisplayString(),
                        sameMapper.ToDisplayString(),
                        mapper.SourceType,
                        mapper.DestType
                        )
                    );
                return false;
            }

            if (mapper.ImplementationType == ImplementationType.Implicit)
            {
                var conflictingMapper = mappers
                    .FirstOrDefault(
                        p => p.ImplementationType == ImplementationType.Implicit 
                            && p.SourceType.Equals(mapper.SourceType, SymbolEqualityComparer.Default)
                        );

                if (conflictingMapper != null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.MapperConflict,
                            model.Locations.FirstOrDefault(),
                            model.ToDisplayString(),
                            conflictingMapper.ToDisplayString(),
                            mapper.SourceType
                            )
                        );
                    return false;
                }
            }

            //if (mapper.SourceType.IsUnboundGenericType || mapper.DestType.IsUnboundGenericType)
            //{
            //    if (mappers.Any(p => p.SourceType.IsUnboundGenericType || p.DestType.IsUnboundGenericType))
            //    {
            //        context.ReportDiagnostic(
            //            Diagnostic.Create(
            //                DiagnosticDescriptors.MultiplyGenericsNotSupported,
            //                model.Locations.FirstOrDefault(),
            //                model.ToDisplayString()
            //                )
            //            );
            //        return false;
            //    }
            //}

            return true;
        }

        private static bool TryBuildKnownMapper(
            INamedTypeSymbol model, 
            AttributeData attr,
            SourceProductionContext context,
            out KnownMapper? result)
        {
            result = null;
            var typeParameters = model.TypeArguments.ToArray().AsSpan();

            var source = (INamedTypeSymbol?)attr.ConstructorArguments[0].Value;
            var dest = (INamedTypeSymbol?)attr.ConstructorArguments[1].Value;

            if (source == null || dest == null)
                return false;

            if (dest.IsStatic)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.DestinationIsStatic,
                        model.Locations.FirstOrDefault(),
                        model.ToDisplayString(),
                        dest.ToDisplayString()
                        )
                    );

                return false;
            }

            var sourceTypeArgsCount = 0;
            var destTypeArgsCount = 0;

            if (source.IsUnboundGenericType)
                sourceTypeArgsCount = source.TypeArguments.Length;

            if (dest.IsUnboundGenericType)
                destTypeArgsCount = dest.TypeArguments.Length;

            if (sourceTypeArgsCount + destTypeArgsCount != typeParameters.Length)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidTypeParametersNumber,
                        model.Locations.FirstOrDefault(),
                        model.ToDisplayString(),
                        typeParameters.Length,
                        sourceTypeArgsCount + destTypeArgsCount
                        )
                    );

                return false;
            }

            if (source.IsUnboundGenericType)
                source = source.OriginalDefinition.Construct(typeParameters.Slice(0, sourceTypeArgsCount).ToArray());

            if (dest.IsUnboundGenericType)
                dest = dest.OriginalDefinition.Construct(typeParameters.Slice(sourceTypeArgsCount, destTypeArgsCount).ToArray());

            string? name = null;
            MissingMappingBehavior missingMappingBehavior = default;
            ImplementationType implementationType = default;
            ConstructorAccessibility constructorAccessibility = default;

            foreach (var mapperNamedArg in attr.NamedArguments)
            {
                if (string.Equals(mapperNamedArg.Key, nameof(MappingGeneratorAttribute.Name), StringComparison.Ordinal))
                {
                    name = (string?)mapperNamedArg.Value.Value;
                    continue;
                }

                if (string.Equals(mapperNamedArg.Key, nameof(MappingGeneratorAttribute.MissingMappingBehavior), StringComparison.Ordinal))
                {
                    missingMappingBehavior = (MissingMappingBehavior)((int?)mapperNamedArg.Value.Value ?? 0);
                    continue;
                }

                if (string.Equals(mapperNamedArg.Key, nameof(MappingGeneratorAttribute.ImplementationType), StringComparison.Ordinal))
                {
                    implementationType = (ImplementationType)((int?)mapperNamedArg.Value.Value ?? 0);
                    continue;
                }

                if (string.Equals(mapperNamedArg.Key, nameof(MappingGeneratorAttribute.ConstructorAccessibility), StringComparison.Ordinal))
                {
                    constructorAccessibility = (ConstructorAccessibility)((int?)mapperNamedArg.Value.Value ?? 0);
                    continue;
                }

                throw new MappingGenerationException($"Unknown named attribute {mapperNamedArg.Key}");
            }

            if (name != null)
            {
                if (!Regex.IsMatch(name, "^[a-zA-Z0-9_]*$"))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidMapperName,
                            model.Locations.FirstOrDefault(),
                            model.ToDisplayString(),
                            name
                            )
                        );

                    return false;
                }
            }

            result = new KnownMapper(model, source, dest, name)
            {
                MissingMappingBehavior = missingMappingBehavior,
                ImplementationType = implementationType,
                ConstructorAccessibility = constructorAccessibility
            };

            return true;
        }
    }
}
