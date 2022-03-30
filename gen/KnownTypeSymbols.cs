using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.CodeAnalysis;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class KnownTypeSymbols
    {
        private Compilation _compilation;

        public INamedTypeSymbol CollectionHelpers { get; }

        public INamedTypeSymbol IEnumerableType { get; }

        public INamedTypeSymbol ICollectionType { get; }

        public INamedTypeSymbol IListType { get; }

        public INamedTypeSymbol CollectionType { get; }

        public INamedTypeSymbol ListType { get; }

        public INamedTypeSymbol HashSetType { get; }

        public KnownTypeSymbols(Compilation compilation)
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));

            _compilation = compilation;

            CollectionHelpers = compilation.GetTypeByMetadataName(typeof(CollectionsHelper).FullName)!;

            IEnumerableType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
            ICollectionType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T);
            IListType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IList_T);
            CollectionType = compilation.GetTypeByMetadataName(typeof(Collection<>).FullName)!;
            ListType = compilation.GetTypeByMetadataName(typeof(List<>).FullName)!;
            HashSetType = compilation.GetTypeByMetadataName(typeof(HashSet<>).FullName)!;
        }

        public bool IsInSameClassHieararchy(ITypeSymbol source, ITypeSymbol destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            if (source.Equals(destination, SymbolEqualityComparer.Default))
                return true;

            var parent = source.BaseType;

            while (parent != null)
            {
                if (parent.Equals(destination, SymbolEqualityComparer.Default))
                    return true;

                parent = parent.BaseType;
            }

            return false;
        }
    }
}
