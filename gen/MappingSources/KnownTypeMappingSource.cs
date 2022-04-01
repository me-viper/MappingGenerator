using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration.MappingSources
{
    internal class KnownTypeMappingSource : BaseMappingSource
    {
        private readonly IReadOnlyCollection<IPropertySymbol> _sourceProperties;

        private KnownMapper Mapper { get; }

        private bool _isInternal;

        public KnownTypeMappingSource(KnownMapper mapper, bool isInternal, MappingEmitContext context) : base(context)
        {
            _sourceProperties = context.SourceProperties;
            _isInternal = isInternal;

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

            var memberName = Context.MemberNamingManager.GetMemberName(Mapper);

            var result = new KnownTypeMappingSpec(memberName, Mapper, entry, _isInternal);
            result.MappingExpressions.Add(MappingSyntaxFactory.CallInnerMapper(memberName, sourceProperty.Name));
            return result;
        }

        private KnownTypeMappingSpec? TryMapCollection(MappingDestination entry, IPropertySymbol sourceProperty)
        {
            var classifier = Context.CollectionClassifier;

            var sourceClassification = classifier.ClassifyCollectionType(sourceProperty.Type);

            if (!sourceClassification.IsEnumerable || sourceClassification.ElementsType == null)
                return null;

            var destClassification = classifier.ClassifyCollectionType(entry.Type);

            if (!destClassification.IsEnumerable || destClassification.ElementsType == null)
                return null;

            if (!Mapper.CanMap(sourceClassification.ElementsType, destClassification.ElementsType))
                return null;

            var memberName = Context.MemberNamingManager.GetMemberName(Mapper);

            var result = new KnownTypeMappingSpec(memberName, Mapper, entry, _isInternal);

            if (entry.EntryType == MappingDestinationType.Property && destClassification.IsCollection)
            {
                MappingSyntaxFactory.CallConvertAndCopyTo(
                    sourceClassification.ElementsType,
                    sourceProperty.Name,
                    destClassification.ElementsType,
                    MappingSyntaxFactory.DestinationMember(entry.Name),
                    destClassification.CollectionType,
                    SyntaxFactory.IdentifierName(memberName),
                    entry.IsWritable(),
                    result.MappingExpressions.Add,
                    result.MappingStatements.Add
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

            result.MappingExpressions.Add(MappingSyntaxFactory.CallInnerMapper(mi, memberName, sourceProperty.Name));

            return result;
        }
    }
}
