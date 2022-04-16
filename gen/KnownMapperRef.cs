
using System;
using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    [DebuggerDisplay("{ToDisplayString(), nq}")]
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

        public virtual string ToDisplayString()
        {
            return $"{SourceType.Name} => {DestType.Name}";
        }
    }
}
