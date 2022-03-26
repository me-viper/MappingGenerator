﻿using System.Collections.Generic;
using System.Linq;

using MappingGenerator.SourceGeneration.Spec;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Talk2Bits.MappingGenerator.SourceGeneration;

namespace MappingGenerator.SourceGeneration.MappingSources
{
    internal class PropertyMappingSource : BaseMappingSource
    {
        private readonly IReadOnlyCollection<IPropertySymbol> _sourceProperties;

        public PropertyMappingSource(MappingGenerationContext context) : base(context)
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

            if (entry.EntryType != MappingDestinationType.Property || !entry.IsReadable())
            {
                ITypeSymbol collectionType;

                if (destClassification.IsType)
                    collectionType = destClassification.CollectionType;
                else
                    collectionType = Context.KnownTypes.ListType.Construct(destClassification.ElementsType);

                if (conv.IsImplicit)
                {
                    var expr = MappingSyntaxFactory.CallCopyToNew(
                        Context.KnownTypes.CollectionHelpers,
                        destClassification.ElementsType,
                        collectionType,
                        sourceProperty.Name
                        );
                    result.MappingExpressions.Add(expr);
                    return result;
                }

                if (converter != null)
                {
                    var expr = MappingSyntaxFactory.CallCopyToNew(
                        Context.KnownTypes.CollectionHelpers,
                        sourceClassification.ElementsType,
                        sourceProperty.Name,
                        destClassification.ElementsType,
                        collectionType,
                        converter.Name
                        );
                    result.MappingExpressions.Add(expr);
                    return result;
                } 
            }

            if (destClassification.IsCollection)
            {
                if (conv.IsImplicit)
                {
                    result.MappingStatements.Add(
                        MappingSyntaxFactory.CallCopyTo(
                            Context.KnownTypes.CollectionHelpers,
                            destClassification.ElementsType,
                            sourceProperty.Name,
                            entry.Name
                            )
                        );
                    return result;
                }

                if (converter != null)
                {
                    result.MappingStatements.Add(
                        MappingSyntaxFactory.CallCopyTo(
                            Context.KnownTypes.CollectionHelpers,
                            sourceClassification.ElementsType,
                            sourceProperty.Name,
                            destClassification.ElementsType,
                            entry.Name,
                            converter.Name
                            )
                        );
                    return result;
                }
            }

            return null;
        }
    }
}
