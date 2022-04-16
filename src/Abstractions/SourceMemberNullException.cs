using System;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    public class SourceMemberNullException : Exception
    {
        public Type SourceType { get; }

        public Type DestinationType { get; }

        public string SourceMember { get; }

        public string DestinationMember { get; }

        public SourceMemberNullException(Type sourceType, string sourceMember, Type destinationType, string destinationMember) 
            : base($"Source {sourceType}.{sourceMember} cannot be null because destination {destinationType}.{destinationMember} is not nullable type.")
        {
            SourceType = sourceType;
            SourceMember = sourceMember;
            DestinationType = destinationType;
            DestinationMember = destinationMember;
        }
    }
}
