using System;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class MappingGeneratorIncludeAssemblyAttribute : Attribute
    {
        public MappingGeneratorIncludeAssemblyAttribute(string assemblyName)
        {
            AssemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));
        }

        public string AssemblyName { get; set; }
    }
}
