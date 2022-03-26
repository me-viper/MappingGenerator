using System;
using System.Collections.Generic;
using System.Linq;

using MappingGenerator.SourceGeneration;
using MappingGenerator.SourceGeneration.Mappers;
using MappingGenerator.SourceGeneration.MappingSources;
using MappingGenerator.SourceGeneration.Spec;

using Microsoft.CodeAnalysis;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class MappingClassGenerator
    {
        private readonly KnownMapper _mapperType;

        private readonly IReadOnlyCollection<KnownMapper> _knownMappers;
        
        public string FileName => _mapperType.Mapper.ToDisplayString().Replace('<', '[').Replace('>', ']');

        public MappingClassGenerator(
            KnownMapper mapper,
            IEnumerable<KnownMapper> knownMappers)
        {
            _mapperType = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _knownMappers = new List<KnownMapper>(knownMappers);
        }

        public SyntaxNode Build(IMappingSourceGeneratorContext executionContext)
        {
            var context = MappingGenerationContext.Build(
                _mapperType.Mapper,
                _mapperType.SourceType,
                _mapperType.DestType,
                _knownMappers,
                executionContext
                );

            var resolvers = new List<BaseMappingSource>();
            
            resolvers.Add(new CustomMethodMappingSource(context));
            resolvers.AddRange(context.KnownMappers.Select(p => new KnownTypeMappingSource(p, context)));
            resolvers.Add(new PropertyMappingSource(context));

            var mapperContextSpec = new ContextSpec();
            var mapperTypeSpec = new MapperTypeSpec();

            var constructorMapper = new ConstructorMapper(resolvers);
            constructorMapper.Map(mapperTypeSpec, context);

            var initOnlyMapper = new InitOnlyPropertyMapper(resolvers);
            initOnlyMapper.Map(mapperTypeSpec, context);

            var propertyMapper = new PropertyMapper(resolvers);
            propertyMapper.Map(mapperTypeSpec, context);

            var model = context.CreateSyntaxModel();
            model.Populate(mapperTypeSpec);

            var mst = new MappingSyntaxFactory();
            var syntax = mst.Build(model);

            if (context.MissingMappingBehaviour != MissingMappingBehavior.Ignore)
            {
                var severity = context.MissingMappingBehaviour == MissingMappingBehavior.Error
                    ? DiagnosticSeverity.Error
                    : DiagnosticSeverity.Warning;

                foreach (var missingMapping in context.DestinationProperties)
                {
                    executionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.MissingMapping(severity),
                            null,
                            context.MapperType.ToDisplayString(),
                            context.DestinationType.ToDisplayString(),
                            missingMapping.Name
                            )
                        );
                }
            }

            return syntax.GetRoot();
        }
    }
}