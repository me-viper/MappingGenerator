using System;
using System.Collections.Generic;

using MappingGenerator.SourceGeneration.MappingSources;

namespace MappingGenerator.SourceGeneration.Mappers
{
    internal class BaseMapper
    {
        protected IEnumerable<BaseMappingSource> MappingSources { get; }

        protected BaseMapper(IEnumerable<BaseMappingSource> mappingSources)
        {
            MappingSources = mappingSources ?? throw new ArgumentNullException(nameof(mappingSources));
        }
    }
}