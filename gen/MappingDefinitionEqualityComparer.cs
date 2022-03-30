using System;
using System.Collections.Generic;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class MappingDefinitionEqualityComparer : IEqualityComparer<MappingDefinition>
    {
        public StringComparison StringComparison { get; init; }

        public static MappingDefinitionEqualityComparer Default { get; } = new() { StringComparison = StringComparison.Ordinal };

        public static MappingDefinitionEqualityComparer IgnoreCase { get; } =
            new() { StringComparison = StringComparison.OrdinalIgnoreCase };

        public bool Equals(MappingDefinition x, MappingDefinition y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            return string.Equals(x.Name, y.Name, StringComparison);
        }

        public int GetHashCode(MappingDefinition obj)
        {
            if (obj == null)
                return 0;

            if (StringComparison == StringComparison.OrdinalIgnoreCase)
                return obj.Name.ToLower().GetHashCode();

            return obj.Name.GetHashCode();
        }
    }
}