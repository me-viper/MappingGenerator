using System;
using System.Collections.Generic;
using System.Linq;

using MappingGenerator.SourceGeneration.Spec;

using Microsoft.CodeAnalysis;

namespace MappingGenerator.SourceGeneration.MappingSources
{
    internal class KnownTypeMappingSource : BaseMappingSource
    {
        private readonly IReadOnlyCollection<IPropertySymbol> _sourceProperties;

        private KnownMapper Mapper { get; }

        public KnownTypeMappingSource(KnownMapper mapper, MappingGenerationContext context) : base(context)
        {
            _sourceProperties = context.SourceProperties;
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public override MappingSpec? TryMap(MappingDestination entry)
        {
            var sourceProperty = _sourceProperties.FirstOrDefault(
                p => string.Equals(p.Name, entry.SourceName, entry.Comparer.StringComparison)
                );

            if (sourceProperty == null)
                return null;

            var collectionSpec = TryMapCollection(entry, sourceProperty);

            if (collectionSpec != null)
                return collectionSpec;

            if (!Mapper.CanMap(sourceProperty.Type, entry.Type))
                return null;

            if (!entry.IsWritable())
                return null;

            var result = new KnownTypeMappingSpec(Mapper, entry);
            result.MappingExpressions.Add(MappingSyntaxFactory.CallInnerMapper(Mapper.Name, sourceProperty.Name));
            return result;
        }

        private KnownTypeMappingSpec? TryMapCollection(MappingDestination entry, IPropertySymbol sourceProperty)
        {
            var classifier = Context.CollectionClassifier;

            var sourceClassification = classifier.ClassifyCollectionType(sourceProperty.Type);

            if (!sourceClassification.IsEnumerable || sourceClassification.ElementsType == null)
                return null;

            var destClassification = classifier.ClassifyCollectionType(entry.Type);

            if (!destClassification.IsType && !destClassification.IsCollection || destClassification.ElementsType == null)
                return null;

            if (!Mapper.CanMap(sourceClassification.ElementsType, destClassification.ElementsType))
                return null;

            var result = new KnownTypeMappingSpec(Mapper, entry);

            if (entry.EntryType != MappingDestinationType.Property || !entry.IsReadable())
            {
                ITypeSymbol collectionType;

                if (destClassification.IsType)
                    collectionType = destClassification.CollectionType;
                else
                    collectionType = Context.KnownTypes.ListType.Construct(destClassification.ElementsType);

                result.MappingExpressions.Add(
                    MappingSyntaxFactory.CallCopyToNew(
                        Context.KnownTypes.CollectionHelpers,
                        sourceClassification.ElementsType,
                        sourceProperty.Name,
                        destClassification.ElementsType,
                        collectionType,
                        Mapper.Name
                        )
                    );
                return result;
            }

            if (destClassification.IsCollection)
            {
                result.MappingStatements.Add(
                    MappingSyntaxFactory.CallCopyTo(
                        Context.KnownTypes.CollectionHelpers,
                        sourceClassification.ElementsType,
                        sourceProperty.Name,
                        destClassification.ElementsType,
                        entry.Name,
                        Mapper.Name
                        )
                    );
                return result;
            }

            if (!entry.IsWritable())
                return null;

            var mi = MappingSyntaxFactory.MapperInterface(
                sourceClassification.CollectionType,
                destClassification.CollectionType,
                destClassification.IsArray
                );

            result.MappingExpressions.Add(MappingSyntaxFactory.CallInnerMapper(mi, Mapper.Name, sourceProperty.Name));

            return result;
        }
    }
}
