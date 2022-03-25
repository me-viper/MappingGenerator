
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using MappingGenerator.Abstractions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MappingGenerator.SourceGeneration
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
            var knownMappers = new HashSet<KnownMapper>();

            foreach (var mapper in classes)
            {
                var semanticModel = compilation.GetSemanticModel(mapper.SyntaxTree);
                var model = ModelExtensions.GetDeclaredSymbol(semanticModel, mapper) as INamedTypeSymbol;

                if (model == null)
                    return;

                var attr = model.GetAttributes().FirstOrDefault(
                    p => string.Equals(p.AttributeClass?.ToDisplayString(), MappingGeneratorAttributeName, StringComparison.Ordinal)
                    );

                if (attr == null)
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

                var typeParameters = model.TypeArguments.ToArray().AsSpan();

                var source = (INamedTypeSymbol?)attr.ConstructorArguments[0].Value;
                var dest = (INamedTypeSymbol?)attr.ConstructorArguments[1].Value;

                if (source == null || dest == null)
                    continue;

                var sourceTypeArgsCount = 0;
                var destTypeArgsCount = 0;

                if (source.IsUnboundGenericType)
                    sourceTypeArgsCount = source.TypeArguments.Count();

                if (dest.IsUnboundGenericType)
                    destTypeArgsCount = dest.TypeArguments.Count();

                if (sourceTypeArgsCount + destTypeArgsCount != typeParameters.Length)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidTypeParametersNumber,
                            mapper.GetLocation(),
                            model.ToDisplayString(),
                            typeParameters.Length,
                            sourceTypeArgsCount + destTypeArgsCount
                            )
                        );
                    continue;
                }

                if (source.IsUnboundGenericType)
                    source = source.OriginalDefinition.Construct(typeParameters.Slice(0, sourceTypeArgsCount).ToArray());

                if (dest.IsUnboundGenericType)
                    dest = dest.OriginalDefinition.Construct(typeParameters.Slice(sourceTypeArgsCount, destTypeArgsCount).ToArray());

                knownMappers.Add(new KnownMapper(model, source, dest));
            }

            foreach (var mapper in knownMappers)
            {
                var generator = new MappingClassGenerator(mapper, knownMappers);
                var generatorContext = new MappingSourceGeneratorContext(compilation, context.ReportDiagnostic);

                try
                {
                    var classSource = generator.Build(generatorContext);
                    context.AddSource($"{generator.FileName}.g.cs", classSource.GetText(Encoding.UTF8));
                }
                catch (MappingGenerationException)
                {
                    // context.ReportDiagnostic()
                }
            }
        }
    }
}
