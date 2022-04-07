using System;
using System.Diagnostics;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.DependencyInjection
{
    [DebuggerDisplay("{Source.Name} => {Destination.Name}")]
    internal readonly struct MapperInfo
    {
        public MapperInfo(Type source, Type destination)
        {
            Source = source;
            Destination = destination;
            MapperType = typeof(IMapper<,>).MakeGenericType(source, destination);
            IsOpenGeneric = source.IsGenericTypeDefinition || destination.IsGenericTypeDefinition;
        }

        public readonly Type Source { get; }

        public readonly Type Destination { get; }

        public readonly Type MapperType { get; }

        public readonly bool IsOpenGeneric { get; }

        public MapperInfo MakeClosedGeneric(MapperInfo closedMapper)
        {
            var sourceArgs = closedMapper.Source.GenericTypeArguments;
            var destArgs = closedMapper.Destination.GenericTypeArguments;

            var closedSourceType = Source.IsGenericTypeDefinition ? Source.MakeGenericType(sourceArgs) : Source;
            var closedDestinationType = Destination.IsGenericTypeDefinition ? Destination.MakeGenericType(destArgs) : Destination;

            return new MapperInfo(closedSourceType, closedDestinationType);
        }

        public bool CanMap(MapperInfo other)
        {
            if (other.MapperType.IsAssignableFrom(MapperType))
                return true;

            if (IsOpenGeneric)
            {
                if (MakeClosedGeneric(other).CanMap(other))
                    return true;
            }

            return false;
        }
    }
}
