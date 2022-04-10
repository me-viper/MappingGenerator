using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using Talk2Bits.MappingGenerator.SourceGeneration.MappingSources;
using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration.Mappers
{
    internal class ConstructorMapper : BaseMapper
    {
        public ConstructorMapper(IEnumerable<BaseMappingSource> mappingSources) : base(mappingSources)
        {
        }

        public void Map(MapperTypeSpec mapperSpec, MappingEmitContext context)
        {
            var destinationConstructor = TryFindCustomConstructor(context);

            if (destinationConstructor != null)
            {
                mapperSpec.HasCustomConstructor = true;
                return; 
            }

            MapperTypeSpec? resolvedSpec = null;
            var hasEmptyConstructor = false;

            foreach (var ctor in context.DestinationType.Constructors.OrderBy(p => p.Parameters.Length))
            {
                if (ctor.Parameters.Length == 0)
                {
                    hasEmptyConstructor = true;
                    continue;
                }

                var hasMatch = false;
                var tempSpec = new MapperTypeSpec(context.ConstructorAccessibility);

                foreach (var param in ctor.Parameters)
                {
                    var dest = MappingDestination.FromDefinition(context.MakeMappingDefinition(param), MappingDestinationType.Parameter);

                    foreach (var mappingSource in MappingSources)
                    {
                        var spec = mappingSource.TryMap(dest);

                        if (spec != null)
                        {
                            tempSpec.ApplySpec(spec);
                            hasMatch = true;
                            break;
                        }
                    }

                    if (hasMatch || param.IsOptional)
                        continue;

                    break;
                }

                if (hasMatch)
                    resolvedSpec = tempSpec;
            }

            if (resolvedSpec == null && !hasEmptyConstructor)
            {
                context.ExecutionContext.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.CantResolveConstructorArgument,
                        context.DestinationType.Locations.FirstOrDefault(),
                        context.MapperType.ToDisplayString(),
                        context.DestinationType.ToDisplayString(),
                        context.SourceType.ToDisplayString()
                        )
                    );

                throw new MappingGenerationException("No constructor");
            }

            if (resolvedSpec != null)
            {
                foreach (var s in resolvedSpec.AppliedSpecs)
                    context.DestinationMapped(s.Destination);

                mapperSpec.Merge(resolvedSpec);
            }
        }

        private static IMethodSymbol? TryFindCustomConstructor(MappingEmitContext context)
        {
            IMethodSymbol? destinationConstructor = null;

            var destinationConstructorCandidates = context.MapperType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(p => p.Name.Equals(context.DestinationConstructorName))
                .ToList();

            foreach (var candidate in destinationConstructorCandidates)
            {
                if (candidate.Parameters.Length != 1
                    || !candidate.Parameters[0].Type.Equals(context.SourceType, SymbolEqualityComparer.Default))
                {
                    context.ExecutionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.BadConstructorMethodSignature,
                            candidate.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            context.SourceType.ToDisplayString(),
                            context.DestinationType.ToDisplayString()
                            )
                        );

                    continue;
                }

                if (!candidate.ReturnType.Equals(context.DestinationType, SymbolEqualityComparer.Default))
                {
                    context.ExecutionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.BadConstructorReturnType,
                            candidate.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            context.DestinationType.ToDisplayString(),
                            candidate.ReturnType.ToDisplayString()
                            )
                        );

                    throw new MappingGenerationException("Bad constructor");
                }

                destinationConstructor = candidate;
                break;
            }

            if (destinationConstructor == null)
            {
                if (context.DestinationType.IsAbstract)
                {
                    context.ExecutionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.DestinationIsAbstractOrInterface,
                            context.DestinationType.Locations.FirstOrDefault(),
                            context.MapperType.ToDisplayString(),
                            context.DestinationType.ToDisplayString()
                            )
                        );

                    throw new MappingGenerationException("Bad constructor");
                }
            }

            return destinationConstructor;
        }
    }
}
