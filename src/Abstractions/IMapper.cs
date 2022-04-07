using System;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    public interface IAbstractMapper
    {
    }

    /// <summary>
    /// Provides a way to map one object instance to other.
    /// </summary>
    /// <typeparam name="TSource">The type of source object.</typeparam>
    /// <typeparam name="TDestination">The type of destination object.</typeparam>
    public interface IMapper<in TSource, out TDestination> : IAbstractMapper
    {
        /// <summary>
        /// Maps given object to the new instance of destination object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>New instance of mapped destination object.</returns>
        TDestination Map(TSource source);
    }
}