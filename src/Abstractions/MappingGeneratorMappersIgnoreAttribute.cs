using System;
using System.Collections.Generic;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MappingGeneratorMappersIgnoreAttribute : Attribute
    {
        public MappingGeneratorMappersIgnoreAttribute(params Type[]? ignore)
        {
            Ignore = ignore ?? Array.Empty<Type>();
        }

        public IEnumerable<Type> Ignore { get; }
    }
}
