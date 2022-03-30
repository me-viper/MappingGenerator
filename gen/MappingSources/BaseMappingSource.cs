using System;

using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration.MappingSources
{
    internal abstract class BaseMappingSource
    {
        protected MappingEmitContext Context { get; }

        protected BaseMappingSource(MappingEmitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public abstract MappingSpec? TryMap(MappingDestination entry);
    }
}
