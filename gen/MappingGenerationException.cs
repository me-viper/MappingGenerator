
using System;
using System.Runtime.Serialization;

namespace MappingGenerator.SourceGeneration
{
    internal class MappingGenerationException : Exception
    {
        public MappingGenerationException()
        {
        }

        public MappingGenerationException(string message) : base(message)
        {
        }

        public MappingGenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MappingGenerationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
