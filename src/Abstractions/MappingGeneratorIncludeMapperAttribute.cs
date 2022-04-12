using System;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class MappingGeneratorIncludeMapperAttribute : Attribute
    {
        public MappingGeneratorIncludeMapperAttribute(params Type[] mappers)
        {
            Mappers = mappers;
        }

        private Type[] Mappers { get; }
    }
}
