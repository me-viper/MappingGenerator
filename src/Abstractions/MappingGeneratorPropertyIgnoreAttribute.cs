using System;
using System.Collections.Generic;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    /// <summary>
    /// Defines which destination properties will be ignored for specified mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MappingGeneratorPropertyIgnoreAttribute : ScopedMapperAttribute
    {
        public MappingGeneratorPropertyIgnoreAttribute(params string[]? ignore)
        {
            Ignore = ignore ?? Array.Empty<string>();
        }

        /// <summary>
        /// List of destination type property names which will be ignored. 
        /// </summary>
        public IEnumerable<string> Ignore { get; }
    }
}
