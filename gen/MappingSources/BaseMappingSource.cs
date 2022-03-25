
using System;

using MappingGenerator.SourceGeneration.Spec;

using Microsoft.CodeAnalysis;

namespace MappingGenerator.SourceGeneration.MappingSources
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
