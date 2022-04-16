using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Talk2Bits.MappingGenerator.Abstractions;
using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal record MapperAnchorSyntaxModel(
        IGeneratorContext Context,
        INamespaceSymbol Namespace, 
        INamedTypeSymbol MapperType,
        KnownTypeSymbols KnownTypes)
    {
        private HashSet<string> _members = new();

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

                if (_members.Contains(memberName))
                    continue;

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

                _members.Add(memberName);
            }

            return true;
        }
    }
}