using System;
using System.Collections.Generic;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    /// <summary>
    /// Specifies which existing generated mappers will be ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MappingGeneratorMappersIgnoreAttribute : ScopedMapperAttribute
    {
        public MappingGeneratorMappersIgnoreAttribute(params Type[]? ignore)
        {
            Ignore = ignore ?? Array.Empty<Type>();
        }

        public IEnumerable<Type> Ignore { get; }
    }
}
