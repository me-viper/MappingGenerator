using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Talk2Bits.MappingGenerator.Abstractions;
using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal record ConstructorOnlySyntaxModel(
        IMappingSourceGeneratorContext Context,
        INamespaceSymbol Namespace,
        INamedTypeSymbol MapperType,
        KnownTypeSymbols KnownTypes)
    {
        private ConstructorAccessibility? _constructorAccessibility;

        public List<FieldDeclarationSyntax> Fields { get; } = new();

        public ConstructorAccessibility ConstructorAccessibility => _constructorAccessibility ?? default;

        public List<ParameterSyntax> ConstructorParameters { get; } = new();

        public List<StatementSyntax> ConstructorBody { get; } = new();

        public virtual bool TryPopulate(MapperTypeSpec mapperSpec, bool isAnchor)
        {
            if (!isAnchor)
                return true;

            if (_constructorAccessibility == null)
                _constructorAccessibility = mapperSpec.ConstructorAccessibility;
            else
            {
                if (_constructorAccessibility != mapperSpec.ConstructorAccessibility)
                {
                    Context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InconsistentAccessibilityModifiers,
                            MapperType.Locations.FirstOrDefault(),
                            MapperType.ToDisplayString()
                            )
                        );
                    return false;
                }
            }

            foreach (var spec in mapperSpec.KnownMappingSpecs)
            {
                var memberName = spec.MemberName;

                Fields.Add(MappingSyntaxFactory.InnerMapperField(spec.Mapper.SourceType, spec.Mapper.DestType, memberName));

                if (!spec.IsInternal)
                {
                    ConstructorParameters.Add(
                        MappingSyntaxFactory.InnerMapperConstructorParameter(spec.Mapper.SourceType, spec.Mapper.DestType, memberName)
                        );
                    ConstructorBody.AddRange(MappingSyntaxFactory.InnerMapperConstructorStatement(memberName));
                }
                else
                {
                    ConstructorBody.AddRange(
                        MappingSyntaxFactory.InnerMapperConstructorThisStatement(
                            spec.Mapper.SourceType,
                            spec.Mapper.DestType,
                            memberName
                            )
                        );
                }
            }

            return true;
        }
    }

    internal record MappingSyntaxModel(
        IMappingSourceGeneratorContext Context,
        string MapperName,
        INamespaceSymbol Namespace,
        INamedTypeSymbol MapperType,
        INamedTypeSymbol SourceType,
        INamedTypeSymbol DestinationType,
        string DestinationConstructorMethodName,
        ImplementationType ImplementationType,
        KnownTypeSymbols KnownTypes) : ConstructorOnlySyntaxModel(Context, Namespace, MapperType, KnownTypes)
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