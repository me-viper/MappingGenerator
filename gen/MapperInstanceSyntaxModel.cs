using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Talk2Bits.MappingGenerator.Abstractions;
using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal record MapperInstanceSyntaxModel(
        IGeneratorContext Context,
        string MapperName,
        INamespaceSymbol Namespace,
        INamedTypeSymbol MapperType,
        INamedTypeSymbol SourceType,
        INamedTypeSymbol DestinationType,
        string DestinationConstructorMethodName,
        ImplementationType ImplementationType,
        KnownTypeSymbols KnownTypes) : MapperAnchorSyntaxModel(Context, Namespace, MapperType, KnownTypes)
    {
        public string AfterMapMethodName => $"{MapperName}AfterMap";

        public LocalFunctionStatementSyntax? DestinationTypeConstructor { get; private set; }

        public List<StatementSyntax> MappingStatements { get; } = new();

        public override bool TryPopulate(MapperTypeSpec mapperSpec, bool isAnchor)
        {
            if (!base.TryPopulate(mapperSpec, isAnchor))
                return false;

            MappingStatements.AddRange(mapperSpec.MappingStatements);

            if (!mapperSpec.HasCustomConstructor)
            {
                var callDestinationConstructor = MappingSyntaxFactory.CallConstructor(
                    DestinationType, 
                    mapperSpec.DestinationConstructorArguments, 
                    mapperSpec.InitStatements
                    );

                DestinationTypeConstructor = MappingSyntaxFactory.CreateMethod(
                    DestinationType,
                    DestinationConstructorMethodName,
                    new[] { callDestinationConstructor }
                    );
            }

            return true;
        }
    }
}