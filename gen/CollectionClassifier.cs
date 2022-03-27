using System;

using Microsoft.CodeAnalysis;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class CollectionClassifier
    {
        private readonly KnownTypeSymbols _knownTypes;

        public CollectionClassifier(KnownTypeSymbols knownTypes)
        {
            _knownTypes = knownTypes ?? throw new ArgumentNullException(nameof(knownTypes));
        }

        public CollectionTypeClassification ClassifyCollectionType(ITypeSymbol type)
        {
            var result = new CollectionTypeClassification(type);

            var namedType = type as INamedTypeSymbol;

            if (namedType == null)
            {
                if (type.TypeKind == TypeKind.Array)
                {
                    if (type is IArrayTypeSymbol ats)
                    {
                        result.IsArray = true;
                        result.IsEnumerable = true;
                        result.ElementsType = ats.ElementType;
                    }
                }

                return result;
            }

            if (!namedType.IsGenericType)
                return new CollectionTypeClassification(type);

            if (namedType.TypeKind == TypeKind.Interface)
                result.IsInterface = true;

            if (namedType.ConstructedFrom.Equals(_knownTypes.IEnumerableType, SymbolEqualityComparer.Default))
            {
                result.IsEnumerable = true;
            }
            
            if (namedType.ConstructedFrom.Equals(_knownTypes.ICollectionType, SymbolEqualityComparer.Default))
            {
                result.IsCollection = true;
            }

            if (namedType.ConstructedFrom.Equals(_knownTypes.IListType, SymbolEqualityComparer.Default))
            {
                result.IsCollection = true;
            }

            if (namedType.ConstructedFrom.Equals(_knownTypes.ListType, SymbolEqualityComparer.Default))
            {
                result.IsEnumerable = true;
                result.IsType = true;
                result.IsCollection = true;
            }

            if (namedType.ConstructedFrom.Equals(_knownTypes.HashSetType, SymbolEqualityComparer.Default))
            {
                result.IsEnumerable = true;
                result.IsType = true;
                result.IsCollection = true;
            }

            if (namedType.ConstructedFrom.Equals(_knownTypes.CollectionType, SymbolEqualityComparer.Default))
            {
                result.IsEnumerable = true;
                result.IsType = true;
                result.IsCollection = true;
            }

            var elementsType = namedType.TypeArguments[0];

            result.ElementsType = elementsType;

            return result;
        }
    }

    [Flags]
    internal enum CollectionTypeInfo
    {
        NotCollection = 0,
        IEnumerable = 2,
        Collection = 4,
        Interface = 8,
        Type = 16,
        Array = 32
    }

    internal record CollectionTypeClassification(ITypeSymbol CollectionType)
    {
        public ITypeSymbol? ElementsType { get; set; }

        public bool IsCollection { get; set; }

        public bool IsEnumerable { get; set; }

        public bool IsInterface { get; set; }

        public bool IsType { get; set; }

        public bool IsArray { get; set; }
    }
}
