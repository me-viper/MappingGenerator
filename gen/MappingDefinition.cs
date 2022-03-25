
using Microsoft.CodeAnalysis;

namespace MappingGenerator.SourceGeneration
{
    internal record MappingDefinition(string Name, ITypeSymbol Type, ISymbol DeclaringSymbol, CustomizedMapping? Options);
}