using System;

using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration.MappingSources
{
    internal abstract class BaseMappingSource
    {
        protected MappingGenerationContext Context { get; }

        protected BaseMappingSource(MappingGenerationContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public abstract MappingSpec? TryMap(MappingDestination entry);
    }
}
