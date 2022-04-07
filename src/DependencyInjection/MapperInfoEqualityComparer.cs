using System.Collections.Generic;

namespace Talk2Bits.MappingGenerator.DependencyInjection
{
    internal class MapperInfoEqualityComparer : IEqualityComparer<MapperInfo>
    {
        public static MapperInfoEqualityComparer Instance { get; } = new();

        public bool Equals(MapperInfo x, MapperInfo y)
        {
            return x.Source == y.Source && x.Destination == y.Destination;
        }

        public int GetHashCode(MapperInfo obj)
        {
            return obj.Source.GetHashCode() ^ obj.Destination.GetHashCode();
        }
    }
}
