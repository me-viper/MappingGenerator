using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration.MappingSources
{
    internal class MemberMapperSource : BaseMappingSource
    {
        private readonly IReadOnlyCollection<IPropertySymbol> _sourceProperties;

        protected KnownMapperRef Mapper { get; }

        public MemberMapperSource(KnownMapperRef mapper, MappingEmitContext context) : base(context)
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

            var memberName = Context.MemberNamingManager.GetMemberName(Mapper);
            var result = new MappingSpec(entry);

            ExpressionSyntax expr;

            if (Mapper.SourceType.NullableAnnotation == sourceProperty.Type.NullableAnnotation)
                expr = MappingSyntaxFactory.CallInnerMapper(memberName, sourceProperty.Name);
            else
            {
                expr = MappingSyntaxFactory.CallInnerMapperNullable(
                    Context.SourceType,
                    Context.DestinationType,
                    sourceProperty.Type,
                    entry.Type,
                    memberName,
                    sourceProperty.Name,
                    entry.Name
                    );
            }

            // If mapper can return nullable type but destination is not nullable.
            if (Mapper.DestType.NullableAnnotation == NullableAnnotation.Annotated 
                && entry.Type.NullableAnnotation != NullableAnnotation.Annotated)
            {
                expr = SyntaxFactory.BinaryExpression(
                    SyntaxKind.CoalesceExpression,
                    expr,
                    MappingSyntaxFactory.ThrowSourceMemberNullException(memberName, Context.DestinationType, entry.Name)
                    );
            }

            result.MappingExpressions.Add(expr);

            return result;
        }

        private MappingSpec? TryMapCollection(MappingDestination entry, IPropertySymbol sourceProperty)
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

            var result = new MappingSpec(entry);

            if (entry.EntryType == MappingDestinationType.Property && destClassification.IsCollection)
            {
                result.MappingStatements.Add(
                    MappingSyntaxFactory.CallConvertAndCopyTo(
                        sourceClassification.ElementsType,
                        sourceProperty.Name,
                        destClassification.ElementsType,
                        MappingSyntaxFactory.DestinationMember(entry.Name),
                        destClassification.CollectionKind,
                        MappingSyntaxFactory.MapperToConverter(SyntaxFactory.IdentifierName(memberName)),
                        entry.IsWritable()
                        )
                    );
                return result;
            }

            if (!entry.IsWritable())
                return null;

            result.MappingExpressions.Add(
                MappingSyntaxFactory.CallInnerMapper(destClassification.CollectionKind, memberName, sourceProperty.Name)
                );

            return result;
        }
    }
}
