using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Talk2Bits.MappingGenerator.SourceGeneration.MappingSources;
using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration.Mappers
{
    internal class InitOnlyPropertyMapper : BaseMapper
    {
        public InitOnlyPropertyMapper(IEnumerable<BaseMappingSource> mappingSources) : base(mappingSources)
        {
        }

        private static MappingDestinationType EntryType => MappingDestinationType.InitProperty;

        private static ExpressionSyntax Transform(string name, ExpressionSyntax expression) =>
             SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(name),
                expression
                );

        public void Map(MapperTypeSpec mapperSpec, MappingEmitContext context)
        {
            foreach (var prop in context.DestinationProperties)
            {
                if (prop.DeclaringSymbol is not IPropertySymbol ps)
                    continue;

                var isInitOnly = ps.SetMethod?.IsInitOnly ?? false;
                
                if (!isInitOnly)
                    continue;

                var dest = MappingDestination.FromDefinition(prop, EntryType);

                foreach (var mappingSource in MappingSources)
                {
                    var spec = mappingSource.TryMap(dest);

                    if (spec != null)
                    {
                        spec.TransformMappingExpressions(p => Transform(prop.Name, p));
                        mapperSpec.ApplySpec(spec);
                        context.DestinationMapped(dest);
                        break;
                    }
                }
            }
        }
    }
}
