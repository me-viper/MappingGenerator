
using System;

using Microsoft.CodeAnalysis;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class KnownMapper : IEquatable<KnownMapper>
    {
        public INamedTypeSymbol Mapper { get; }

        public INamedTypeSymbol SourceType { get; }

        public INamedTypeSymbol DestType { get; }

        public string Name { get; }

        public string? LocalName { get; }

        public KnownMapper(
            INamedTypeSymbol mapper, 
            INamedTypeSymbol sourceType, 
            INamedTypeSymbol destType,
            string? name)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            DestType = destType ?? throw new ArgumentNullException(nameof(destType));

            LocalName = name;
            Name = mapper.Name + (name ?? string.Empty);
        }

        public MissingMappingBehavior MissingMappingBehavior { get; init; }

        public ImplementationType ImplementationType { get; init; }

        public ConstructorAccessibility ConstructorAccessibility { get; init; }

        public bool CanMap(ITypeSymbol sourceType, ITypeSymbol destType)
        {
            return SourceType.Equals(sourceType, SymbolEqualityComparer.Default)
                && DestType.Equals(destType, SymbolEqualityComparer.Default);
        }

        public bool Equals(KnownMapper other)
        {
            if (other == null)
                return false;

            return Mapper.Equals(other.Mapper, SymbolEqualityComparer.Default)
                && SourceType.Equals(other.SourceType, SymbolEqualityComparer.Default)
                && DestType.Equals(other.DestType, SymbolEqualityComparer.Default);
        }

        public string ToDisplayString()
        {
            var localName = LocalName != null ? $" (Name = {LocalName})" : string.Empty;
            return $"{SourceType.Name} to {DestType.Name}{localName}";
        }
    }
}
