using System.Collections.Generic;

namespace MappingGenerator.SourceGeneration.Spec
{
    internal class ContextSpec
    {
        public List<MapperTypeSpec> Mappers { get; } = new();
    }
}