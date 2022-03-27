using System;
using System.Collections.Generic;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class MappingEntryEqualityComparer : IEqualityComparer<MappingDefinition>
    {
        public StringComparison StringComparison { get; init; }

        public static MappingEntryEqualityComparer Default { get; } = new() { StringComparison = StringComparison.Ordinal };

        public static MappingEntryEqualityComparer IgnoreCase { get; } =
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