using System.Collections.Generic;

namespace Talk2Bits.MappingGenerator.SourceGeneration.Spec
{
    internal class ContextSpec
    {
        public List<MapperTypeSpec> Mappers { get; } = new();
    }
}