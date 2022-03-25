
using Microsoft.CodeAnalysis;

namespace MappingGenerator.SourceGeneration
{
    internal static class DiagnosticDescriptors
    {
        public static DiagnosticDescriptor NoCast => new(
            "MG0001",
            "Error in mapping generator",
            "Mapping generator '{0}': Can't map '{1}' with type '{2}' to destination with type '{3}'. No cast exists.",
            "MappingGenerator",
            DiagnosticSeverity.Error,
            true
            );

        public static DiagnosticDescriptor NotPartial => new(
            "MG0002",
            "Error in mapping generator",
            "Mapping generator '{0}': Mapping class has partial modifier missing.",
            "MappingGenerator",
            DiagnosticSeverity.Error,
            true
            );

        public static DiagnosticDescriptor BadMappingMethodSignature => new(
            "MG0003",
            "Invalid mapping method signature",
            "Mapping generator '{0}': Mapping method expected to have single parameter with type {1} and non void return type. It will be ignored.",
            "MappingGenerator",
            DiagnosticSeverity.Warning,
            true
            );

        public static DiagnosticDescriptor MissingMapping(DiagnosticSeverity severity) => new(
            "MG0004",
            "Missing mapping",
            "Mapping generator '{0}': Failed to resolve mapping for type '{1}' property '{2}'.",
            "MappingGenerator",
            severity,
            true
            );

        public static DiagnosticDescriptor InvalidProperty => new(
            "MG0005",
            "Invalid configuration",
            "Mapping generator '{0}': {1} type '{2}' does not contain property '{3}'.",
            "MappingGenerator",
            DiagnosticSeverity.Error,
            true
            );

        public static DiagnosticDescriptor BadConstructorMethodSignature => new(
            "MG0006",
            "Invalid destination constructor method signature",
            "Mapping generator '{0}': Destination constructor method expected to have single parameter with type '{1}' and return type '{2}'. It will be ignored.",
            "MappingGenerator",
            DiagnosticSeverity.Warning,
            true
            );

        public static DiagnosticDescriptor CantBeNestedClass => new(
            "MG0007",
            "Invalid mapping generator",
            "Mapping generator '{0}': Mapping generator can't be nested class.",
            "MappingGenerator",
            DiagnosticSeverity.Error,
            true
            );

        public static DiagnosticDescriptor CantResolveConstructorArgument => new(
            "MG0008",
            "Failed to map constructor",
            "Mapping generator '{0}': Destination type '{1}' has no constructors that can be resolved with source type '{2}'.",
            "MappingGenerator",
            DiagnosticSeverity.Error,
            true
            );

        public static DiagnosticDescriptor BadConstructorReturnType => new(
            "MG0009",
            "Invalid destination constructor method signature",
            "Mapping generator '{0}': Expected destination constructor method to have return type '{1}' but got '{2}'.",
            "MappingGenerator",
            DiagnosticSeverity.Error,
            true
            );

        public static DiagnosticDescriptor InvalidTypeParametersNumber => new(
            "MG0010",
            "Invalid type parameters number",
            "Mapping generator '{0}': Mapping generator has {1} type parameters specified but {2} expected.",
            "MappingGenerator",
            DiagnosticSeverity.Error,
            true
            );
    }
}
