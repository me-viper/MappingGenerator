using System.Collections.Generic;

using MappingGenerator.SourceGeneration.MappingSources;
using MappingGenerator.SourceGeneration.Spec;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MappingGenerator.SourceGeneration.Mappers
{
    internal class PropertyMapper : BaseMapper
    {
        public PropertyMapper(IEnumerable<BaseMappingSource> mappingSources) : base(mappingSources)
        {
        }

        private MappingDestinationType EntryType => MappingDestinationType.Property;

        private static StatementSyntax MakeStatement(string name, ExpressionSyntax expression) =>
            MappingSyntaxFactory.PropertyMapping(name, expression);

        public void Map(MapperTypeSpec mapperSpec, MappingGenerationContext context)
        {
            foreach (var prop in context.DestinationProperties)
            {
                var dest = MappingDestination.FromDefinition(prop, EntryType);

                foreach (var mappingSource in MappingSources)
                {
                    var spec = mappingSource.TryMap(dest);

                    if (spec != null)
                    {
                        mapperSpec.ApplySpec(spec, p => MakeStatement(prop.Name, p));
                        context.DestinationMapped(dest);
                        break;
                    }
                }
            }
        }
    }
}