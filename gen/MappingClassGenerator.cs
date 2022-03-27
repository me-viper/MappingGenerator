using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using Talk2Bits.MappingGenerator.Abstractions;
using Talk2Bits.MappingGenerator.SourceGeneration.Mappers;
using Talk2Bits.MappingGenerator.SourceGeneration.MappingSources;
using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class MappingClassGenerator
    {
        private readonly INamedTypeSymbol _mapperType;

        private readonly IReadOnlyCollection<KnownMapper> _internalMappers;

        private readonly IReadOnlyCollection<KnownMapper> _knownMappers;

        private readonly MemberNamingManager _memberNamingManager = new();

        public string FileName => _mapperType.ToDisplayString().Replace('<', '[').Replace('>', ']');

        public MappingClassGenerator(
            INamedTypeSymbol mapper,
            IReadOnlyCollection<KnownMapper> internalMappers,
            IEnumerable<KnownMapper> knownMappers)
        {
            if (internalMappers == null)
                throw new ArgumentNullException(nameof(internalMappers));

            _mapperType = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _knownMappers = new List<KnownMapper>(knownMappers);

            var mappers = new List<KnownMapper>(internalMappers.Count);

            foreach (var im in internalMappers)
                mappers.Add(im);

            _internalMappers = mappers;
        }

        public IEnumerable<SyntaxNode> Build(IMappingSourceGeneratorContext executionContext)
        {
            var mapperContextSpec = new ContextSpec();
            var mst = new MappingSyntaxFactory();

            var result = new List<SyntaxNode>();

            if (_internalMappers.Count == 1)
            {
                var model = BuildMapper(_internalMappers.Single(), executionContext);

                if (model == null)
                    return new List<SyntaxNode>(0);

                var syntax = mst.Build(model).GetRoot();
                result.Add(syntax);
                return result;
            }

            var emitContext = EmitContext.Build(_mapperType, executionContext);
            var anchorClassModel = emitContext.CreateSyntaxXModel();

            // 1. Merge constructors.
            // 2. Generate separate partial class with constructor only.
            // 3. PROFIT!
            foreach (var target in _internalMappers)
            {
                var model = BuildMapper(target, executionContext, p => anchorClassModel.TryPopulate(p, true));

                if (model == null)
                    return new List<SyntaxNode>(0);

                var syntax = mst.Build(model).GetRoot();
                result.Add(syntax);
            }

            var anchorSyntax = mst.BuildConstructorOnly(anchorClassModel).GetRoot();
            result.Add(anchorSyntax);

            return result;
        }

        private MappingSyntaxModel? BuildMapper(
            KnownMapper mapperToGenerate, 
            IMappingSourceGeneratorContext executionContext,
            Func<MapperTypeSpec, bool>? populate = null)
        {
            var context = MappingGenerationContext.Build(
                mapperToGenerate,
                mapperToGenerate.SourceType,
                mapperToGenerate.DestType,
                _internalMappers,
                _knownMappers,
                executionContext,
                _memberNamingManager
                );

            var resolvers = new List<BaseMappingSource>();

            resolvers.Add(new CustomMethodMappingSource(context));
            resolvers.AddRange(context.InternalMappers.Select(p => new KnownTypeMappingSource(p, true, context)));
            resolvers.AddRange(context.KnownMappers.Select(p => new KnownTypeMappingSource(p, false, context)));
            resolvers.Add(new PropertyMappingSource(context));

            var mapperTypeSpec = new MapperTypeSpec(context.ConstructorAccessibility);

            var constructorMapper = new ConstructorMapper(resolvers);
            constructorMapper.Map(mapperTypeSpec, context);

            var initOnlyMapper = new InitOnlyPropertyMapper(resolvers);
            initOnlyMapper.Map(mapperTypeSpec, context);

            var propertyMapper = new PropertyMapper(resolvers);
            propertyMapper.Map(mapperTypeSpec, context);

            var model = context.CreateSyntaxModel();
            
            if (!model.TryPopulate(mapperTypeSpec, populate == null))
                return null;

            if (populate != null)
            {
                if (!populate(mapperTypeSpec))
                    return null;
            }

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

            return model;
        }
    }
}