using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration.MappingSources
{
    internal class PropertyMappingSource : BaseMappingSource
    {
        private readonly IReadOnlyCollection<IPropertySymbol> _sourceProperties;

        public PropertyMappingSource(MappingEmitContext context) : base(context)
        {
            _sourceProperties = context.SourceProperties;
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

            if (!entry.IsWritable())
                return null;

            var result = new MappingSpec(entry);

            var expr = MappingSyntaxFactory.PropertyMappingExpression(sourceProperty.Name);
            var converter = Context.TryGetTypeConverter(sourceProperty.Type, entry.Type);

            if (converter != null)
                expr = MappingSyntaxFactory.CallConvertMethod(converter.Name, expr);
            else
            {
                var conv = Context.ExecutionContext.Compilation.ClassifyConversion(sourceProperty.Type, entry.Type);

                if (!conv.Exists)
                    return null;

                if (conv.IsExplicit)
                    expr = SyntaxFactory.CastExpression(MappingSyntaxFactory.GetTypeSyntax(entry.Type), expr);

                expr = MappingSyntaxFactory.NullAwareExpression(
                    Context.SourceType, 
                    Context.DestinationType,
                    sourceProperty.Type,
                    entry.Type,
                    expr, 
                    sourceProperty.Name, 
                    entry.Name, 
                    false
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

            var converter = Context.TryGetTypeConverter(sourceClassification.ElementsType, destClassification.ElementsType);

            var conv = Context.ExecutionContext.Compilation.ClassifyConversion(
                sourceClassification.ElementsType,
                destClassification.ElementsType
                );

            var result = new MappingSpec(entry);

            if (entry.EntryType == MappingDestinationType.Property && entry.IsReadable() && destClassification.IsCollection)
            {
                if (conv.Exists)
                {
                    if (conv.IsImplicit)
                    {
                        if (Context.KnownTypes.IsInSameClassHierarchy(sourceClassification.ElementsType, destClassification.ElementsType))
                        {
                            result.MappingStatements.Add(
                                MappingSyntaxFactory.CallCopyTo(
                                    destClassification.ElementsType,
                                    sourceProperty.Name,
                                    MappingSyntaxFactory.DestinationMember(entry.Name),
                                    destClassification.CollectionKind,
                                    entry.IsWritable()
                                    )
                                );
                            return result;
                        }
                    }

                    result.MappingStatements.Add(
                        MappingSyntaxFactory.CallConvertAndCopyTo(
                            sourceClassification.ElementsType,
                            sourceProperty.Name,
                            destClassification.ElementsType,
                            MappingSyntaxFactory.DestinationMember(entry.Name),
                            destClassification.CollectionKind,
                            MappingSyntaxFactory.ExplicitCastConverter(destClassification.ElementsType),
                            entry.IsWritable()
                            )
                        );
                    return result;
                }

                if (converter != null)
                {
                    result.MappingStatements.Add(
                        MappingSyntaxFactory.CallConvertAndCopyTo(
                            sourceClassification.ElementsType,
                            sourceProperty.Name,
                            destClassification.ElementsType,
                            MappingSyntaxFactory.DestinationMember(entry.Name),
                            destClassification.CollectionKind,
                            SyntaxFactory.IdentifierName(converter.Name),
                            entry.IsWritable()
                            )
                        );
                    return result;
                }
            }

            if (!entry.IsWritable())
                return null;

            if (converter != null)
            {
                var expr = MappingSyntaxFactory.CallConvertAndCopyToNew(
                    sourceClassification.ElementsType,
                    sourceProperty.Name,
                    destClassification.ElementsType,
                    destClassification.CollectionKind,
                    SyntaxFactory.IdentifierName(converter.Name)
                    );
                result.MappingExpressions.Add(expr);
                return result;
            }

            if (!conv.Exists)
                return null;

            // If we have implicit convertion A => B, doesn't mean we have convertion IEnumerable<A> => IEnumerable<B>.
            if (conv.IsImplicit)
            {
                if (Context.KnownTypes.IsInSameClassHierarchy(sourceClassification.ElementsType, destClassification.ElementsType))
                {
                    var expr = MappingSyntaxFactory.CallCopyToNew(
                        destClassification.ElementsType,
                        destClassification.CollectionKind,
                        sourceProperty.Name
                        );
                    result.MappingExpressions.Add(expr);
                    return result;
                }
            }

            result.MappingExpressions.Add(
                MappingSyntaxFactory.CallConvertAndCopyToNew(
                    sourceClassification.ElementsType,
                    sourceProperty.Name,
                    destClassification.ElementsType,
                    destClassification.CollectionKind,
                    MappingSyntaxFactory.ExplicitCastConverter(destClassification.ElementsType)
                    )
                );

            return result;
        }
    }
}
