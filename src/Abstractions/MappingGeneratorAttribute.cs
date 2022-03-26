using System;

namespace MappingGenerator.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MappingGeneratorAttribute : Attribute
    {
        public MappingGeneratorAttribute(Type source, Type destination)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        }

        public Type Source { get; }

        public Type Destination { get; }

        public MissingMappingBehavior MissingMappingBehavior { get; set; }

        public ImplementationType ImplementationType { get; set; }

        public ConstructorAccessibility ConstructorAccessibility { get; set; } = ConstructorAccessibility.Public;

        public string Name { get; set; } = string.Empty;
    }
}
