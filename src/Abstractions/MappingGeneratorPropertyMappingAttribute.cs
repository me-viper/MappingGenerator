using System;

namespace Talk2Bits.MappingGenerator.Abstractions
{

    /// <summary>
    /// Defines mapping correspondence between source type property and destination type property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MappingGeneratorPropertyMappingAttribute : ScopedMapperAttribute
    {
        public MappingGeneratorPropertyMappingAttribute(string source, string destination)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Value can't be null or empty string", nameof(source));

            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Value can't be null or empty string", nameof(destination));

            Source = source;
            Destination = destination;
        }

        /// <summary>
        /// Source property.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Destination property. 
        /// </summary>
        public string Destination { get; }
    }
}
