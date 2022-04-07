using System;

namespace Talk2Bits.MappingGenerator.DependencyInjection
{
    public class UnknownMappingException : Exception
    {
        public Type SourceType { get; }

        public Type DestinationType { get; }

        internal UnknownMappingException(Type source, Type destination) 
            : base($"No mapper '{source.FullName}' => '{destination.FullName}' found")
        {
            SourceType = source;
            DestinationType = destination;
        }
    }
}
