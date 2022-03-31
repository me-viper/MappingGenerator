using System;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    public abstract class ScopedMapperAttribute : Attribute
    {
        public string? AppliesTo { get; set; }
    }
}