using System;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    public class MappingNullException : Exception
    {
        public Type SourceType { get; }

        public Type DestinationType { get; }

        public string SourceMember { get; }

        public string DestinationMember { get; }

        public MappingNullException(Type sourceType, string sourceMember, Type destinationType, string destinationMember) 
            : base($"Source {sourceType}.{sourceMember} cannot be null because destination {destinationType}.{destinationMember} is not nullable type.")
        {
            SourceType = sourceType;
            SourceMember = sourceMember;
            DestinationType = destinationType;
            DestinationMember = destinationMember;
        }

        public MappingNullException(Type mapperType, Type destinationType, string destinationMember)
            : base($"Mapper {mapperType} returned 'null' but destination {destinationType}.{destinationMember} is not nullable type.")
        {
            SourceType = mapperType;
            DestinationType = destinationType;
            SourceMember = "Map";
            DestinationMember = destinationMember;
        }
     }
}
