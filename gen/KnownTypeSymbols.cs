using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.CodeAnalysis;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class KnownTypeSymbols
    {
        public KnownTypeSymbols(Compilation compilation)
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));

            CollectionHelpers = compilation.GetTypeByMetadataName(typeof(CollectionsHelper).FullName)!;

            IEnumerableType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
            ICollectionType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T);
            IListType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IList_T);
            CollectionType = compilation.GetTypeByMetadataName(typeof(Collection<>).FullName)!;
            ListType = compilation.GetTypeByMetadataName(typeof(List<>).FullName)!;
            HashSetType = compilation.GetTypeByMetadataName(typeof(HashSet<>).FullName)!;
        }

        public INamedTypeSymbol CollectionHelpers { get; }

        public INamedTypeSymbol IEnumerableType { get; }

        public INamedTypeSymbol ICollectionType { get; }

        public INamedTypeSymbol IListType { get; }

        public INamedTypeSymbol CollectionType { get; }

        public INamedTypeSymbol ListType { get; }

        public INamedTypeSymbol HashSetType { get; }
    }
}
