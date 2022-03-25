using System.Collections.Generic;

namespace MappingGenerator.SourceGeneration.Spec
{
    internal class KnownTypeMappingSpecEqualityComparer : IEqualityComparer<KnownTypeMappingSpec>
    {
        public static KnownTypeMappingSpecEqualityComparer Default { get; } = new();

        public bool Equals(KnownTypeMappingSpec x, KnownTypeMappingSpec y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            if (x.Mapper == null || y.Mapper == null)
                return false;

            return x.Mapper.Equals(y.Mapper);
        }

        public int GetHashCode(KnownTypeMappingSpec obj)
        {
            if (obj == null)
                return 0;

            return obj.Mapper.GetHashCode();
        }
    }
}