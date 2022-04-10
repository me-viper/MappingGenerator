
using System;

using Microsoft.CodeAnalysis;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class KnownMapperRef : IEquatable<KnownMapperRef>
    {
        public INamedTypeSymbol Mapper { get; }

        public INamedTypeSymbol SourceType { get; }

        public INamedTypeSymbol DestType { get; }

        public string MemberName { get; }

        public KnownMapperRef(
            INamedTypeSymbol mapper,
            INamedTypeSymbol sourceType,
            INamedTypeSymbol destType,
            string memberName)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            DestType = destType ?? throw new ArgumentNullException(nameof(destType));
            MemberName = memberName;
        }

        public bool CanMap(ITypeSymbol sourceType, ITypeSymbol destType)
        {
            return SourceType.Equals(sourceType, SymbolEqualityComparer.Default)
                && DestType.Equals(destType, SymbolEqualityComparer.Default);
        }

        public bool Equals(KnownMapperRef other)
        {
            if (other == null)
                return false;

            return Mapper.Equals(other.Mapper, SymbolEqualityComparer.Default)
                && SourceType.Equals(other.SourceType, SymbolEqualityComparer.Default)
                && DestType.Equals(other.DestType, SymbolEqualityComparer.Default);
        }
    }

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

        public string ToDisplayString()
        {
            var localName = LocalName != null ? $" (Name = {LocalName})" : string.Empty;
            return $"{SourceType.Name} to {DestType.Name}{localName}";
        }
    }
}
