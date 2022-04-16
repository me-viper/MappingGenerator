
using System;

using Microsoft.CodeAnalysis;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class KnownMapper : KnownMapperRef, IEquatable<KnownMapper>
    {
        public string? LocalName { get; }

        public KnownMapper(
            INamedTypeSymbol mapper, 
            INamedTypeSymbol sourceType, 
            INamedTypeSymbol destType,
            string? localName) : base(mapper, sourceType, destType, MapperMemberName(mapper.Name, localName))
        {
            LocalName = localName;
        }

        public MissingMappingBehavior MissingMappingBehavior { get; init; }

        public ImplementationType ImplementationType { get; init; }

        public ConstructorAccessibility ConstructorAccessibility { get; init; }

        public static string MapperMemberName(string baseName, string? localName)
        {
            return $"{char.ToLower(baseName[0])}{baseName.Substring(1)}" + (localName ?? string.Empty);
        }

        public bool Equals(KnownMapper other)
        {
            if (other == null)
                return false;

            return Mapper.Equals(other.Mapper, SymbolEqualityComparer.Default)
                && SourceType.Equals(other.SourceType, SymbolEqualityComparer.Default)
                && DestType.Equals(other.DestType, SymbolEqualityComparer.Default);
        }

        public override string ToDisplayString()
        {
            var localName = LocalName != null ? $" (Name = {LocalName})" : string.Empty;
            return $"{SourceType.Name} => {DestType.Name}{localName}";
        }
    }
}
