using System;

namespace MappingGenerator.Abstractions
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MappingGeneratorPropertyMappingAttribute : Attribute
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

        public string Source { get; }

        public string Destination { get; }
    }
}
