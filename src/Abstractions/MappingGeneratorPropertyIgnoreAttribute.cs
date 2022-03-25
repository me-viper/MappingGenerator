using System;
using System.Collections.Generic;

namespace MappingGenerator.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MappingGeneratorPropertyIgnoreAttribute : Attribute
    {
        public MappingGeneratorPropertyIgnoreAttribute(params string[]? ignore)
        {
            Ignore = ignore ?? Array.Empty<string>();
        }

        public IEnumerable<string> Ignore { get; }
    }
}
