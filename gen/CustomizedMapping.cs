
using Microsoft.CodeAnalysis;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal record CustomizedMapping(
        IPropertySymbol Source
        );
}