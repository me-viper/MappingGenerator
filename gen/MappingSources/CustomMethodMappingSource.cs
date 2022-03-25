using System.Collections.Generic;
using System.Linq;

using MappingGenerator.SourceGeneration.Spec;

using Microsoft.CodeAnalysis;

namespace MappingGenerator.SourceGeneration.MappingSources
{
    internal class CustomMethodMappingSource : BaseMappingSource
    {
        private readonly IReadOnlyCollection<IMethodSymbol> _mappingMethods;

        public CustomMethodMappingSource(MappingGenerationContext context) : base(context)
        {
            _mappingMethods = context.MappingMethods;
        }

        public override MappingSpec? TryMap(MappingDestination entry)
        {
            if (!entry.IsWritable())
                return null;

            var name = Context.MapMethodName(entry.SourceName);

            var sourceMethod = _mappingMethods.FirstOrDefault(p => string.Equals(p.Name, name, entry.Comparer.StringComparison));

            if (sourceMethod == null)
                return null;

            var result = new MappingSpec(entry);
            result.MappingExpressions.Add(MappingSyntaxFactory.CallMappingMethod(name));
            return result;
        }
    }
}
