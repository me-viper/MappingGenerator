
using Microsoft.CodeAnalysis;

namespace MappingGenerator.SourceGeneration
{
    internal record CustomizedMapping(
        IPropertySymbol Source
        );
}