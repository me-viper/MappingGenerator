using System.Collections.Generic;

using MappingGenerator.SourceGeneration.Spec;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MappingGenerator.SourceGeneration
{
    internal record MappingSyntaxModel(
        MappingGenerationContext Context,
        INamespaceSymbol Namespace,
        INamedTypeSymbol MapperType,
        INamedTypeSymbol SourceType,
        INamedTypeSymbol DestinationType)
    {
        public MethodDeclarationSyntax? DestinationTypeConstructor { get; private set; }

        public List<FieldDeclarationSyntax> Fields { get; } = new();

        public List<ParameterSyntax> ConstructorParameters { get; } = new();

        public List<StatementSyntax> ConstructorBody { get; } = new();

        public List<StatementSyntax> MappingStatements { get; } = new();

        public void Populate(MapperTypeSpec mapperSpec)
        {
            MappingStatements.AddRange(mapperSpec.MappingStatements);

            foreach (var spec in mapperSpec.KnownMappingSpecs)
            {
                var memberName = char.ToLower(spec.Mapper.Name[0]) + spec.Mapper.Name.Substring(1);
                Fields.Add(MappingSyntaxFactory.InnerMapperField(spec.Mapper.SourceType, spec.Mapper.DestType, memberName));
                ConstructorParameters.Add(
                    MappingSyntaxFactory.InnerMapperConstructorParameter(spec.Mapper.SourceType, spec.Mapper.DestType, memberName)
                    );
                ConstructorBody.AddRange(MappingSyntaxFactory.InnerMapperConstructorStatement(memberName));
            }

            if (!mapperSpec.HasCustomConstructor)
            {
                var callDestinationConstructor = MappingSyntaxFactory.CallConstructor(
                    DestinationType, 
                    mapperSpec.DestinationConstructorArguments, 
                    mapperSpec.InitStatements
                    );

                DestinationTypeConstructor = MappingSyntaxFactory.CreateMethod(
                    SourceType,
                    DestinationType,
                    Context.DestinationConstructorMethodName,
                    new[] { callDestinationConstructor }
                    );
            }
        }
    }
}