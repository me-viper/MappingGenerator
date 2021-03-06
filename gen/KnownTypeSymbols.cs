using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        
        public INamedTypeSymbol IDictionaryType { get; }
        
        public INamedTypeSymbol DictionaryType { get; }

        public INamedTypeSymbol KeyValueType { get; }

        public INamedTypeSymbol IMapper { get; }
        
        public KnownTypeSymbols(Compilation compilation)
        {
            _compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));

            CollectionHelpers = compilation.GetTypeByMetadataName(typeof(CollectionsHelper).FullName)!;

            IEnumerableType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
            ICollectionType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T);
            IListType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IList_T);
            CollectionType = compilation.GetTypeByMetadataName(typeof(Collection<>).FullName)!;
            ListType = compilation.GetTypeByMetadataName(typeof(List<>).FullName)!;
            HashSetType = compilation.GetTypeByMetadataName(typeof(HashSet<>).FullName)!;
            DictionaryType = compilation.GetTypeByMetadataName(typeof(Dictionary<,>).FullName)!;
            IDictionaryType = compilation.GetTypeByMetadataName(typeof(IDictionary<,>).FullName)!;
            IMapper = compilation.GetTypeByMetadataName(typeof(IMapper<,>).FullName)!;
            KeyValueType = compilation.GetTypeByMetadataName(typeof(KeyValuePair<,>).FullName)!;
        }

        public bool IsInSameClassHierarchy(ITypeSymbol source, ITypeSymbol destination)
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
