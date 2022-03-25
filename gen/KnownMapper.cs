
using System;

using Microsoft.CodeAnalysis;

namespace MappingGenerator.SourceGeneration
{
    internal class KnownMapper : IEquatable<KnownMapper>
    {
        public INamedTypeSymbol Mapper { get; }

        public INamedTypeSymbol SourceType { get; }

        public INamedTypeSymbol DestType { get; }

        public string Name { get; }

        public KnownMapper(INamedTypeSymbol mapper, INamedTypeSymbol sourceType, INamedTypeSymbol destType)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            DestType = destType ?? throw new ArgumentNullException(nameof(destType));

            Name = $"{char.ToLower(Mapper.Name[0])}{Mapper.Name.Substring(1)}";
        }

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
    }
}
