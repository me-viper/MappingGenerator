using System;

using Microsoft.CodeAnalysis;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class EmitContext
    {
        public INamedTypeSymbol MapperType { get; }

        public IGeneratorContext ExecutionContext { get; }

        public KnownTypeSymbols KnownTypes { get; }

        public CollectionClassifier CollectionClassifier { get; }

        protected EmitContext(INamedTypeSymbol mapperType, IGeneratorContext executionContext)
        {
            MapperType = mapperType ?? throw new ArgumentNullException(nameof(mapperType));
            ExecutionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
            KnownTypes = new KnownTypeSymbols(executionContext.Compilation);
            CollectionClassifier = new CollectionClassifier(KnownTypes);
        }

        public static EmitContext Build(
            INamedTypeSymbol mapperType, 
            IGeneratorContext executionContext)
        {
            var result = new EmitContext(mapperType, executionContext);
            return result;
        }

        public MapperAnchorSyntaxModel CreateAnchorSyntaxModel()
        {
            return new MapperAnchorSyntaxModel(
                ExecutionContext,
                MapperType.ContainingNamespace,
                MapperType,
                KnownTypes
                );
        }
    }
}