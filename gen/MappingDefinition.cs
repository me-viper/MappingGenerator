
using Microsoft.CodeAnalysis;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal record MappingDefinition(string Name, ITypeSymbol Type, ISymbol DeclaringSymbol, CustomizedMapping? Options);
}