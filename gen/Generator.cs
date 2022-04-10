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
    internal class Generator
    {
        private readonly INamedTypeSymbol _mapperType;

        private readonly IReadOnlyCollection<KnownMapper> _internalMappers;

        private readonly IReadOnlyCollection<KnownMapper> _knownMappers;

        public string FileName => _mapperType.ToDisplayString().Replace('<', '[').Replace('>', ']');

        public Generator(
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

        public IEnumerable<SyntaxNode> Build(IGeneratorContext executionContext)
        {
            var mst = new MappingSyntaxFactory();
            var result = new List<SyntaxNode>();

            var emitContext = EmitContext.Build(
                _mapperType, 
                executionContext,
                _internalMappers,
                _knownMappers
                );

            if (_internalMappers.Count == 1)
            {
                var model = BuildMapper(_internalMappers.Single(), emitContext);

                if (model == null)
                    return new List<SyntaxNode>(0);

                var syntax = mst.Build(model).GetRoot();
                result.Add(syntax);
                return result;
            }
            
            var anchorClassModel = emitContext.CreateSyntaxModel();

            // 1. Merge constructors.
            // 2. Generate separate partial class with constructor only.
            // 3. PROFIT!
            foreach (var target in _internalMappers)
            {
                var model = BuildMapper(target, emitContext, p => anchorClassModel.TryPopulate(p, true));

                if (model == null)
                    return new List<SyntaxNode>(0);

                var syntax = mst.Build(model).GetRoot();
                result.Add(syntax);
            }

            var ctorSyntax = mst.BuildConstructorOnly(anchorClassModel);

            if (ctorSyntax != null)
            {
                var anchorSyntax = ctorSyntax.GetRoot();
                result.Add(anchorSyntax); 
            }

            return result;
        }

        private MapperInstanceSyntaxModel? BuildMapper(
            KnownMapper mapperToGenerate, 
            EmitContext emitContext,
            Func<MapperTypeSpec, bool>? populate = null)
        {
            var context = MappingEmitContext.Build(
                emitContext,
                mapperToGenerate,
                mapperToGenerate.SourceType,
                mapperToGenerate.DestType
                );

            var resolvers = new List<BaseMappingSource>();

            resolvers.Add(new CustomMethodMappingSource(context));
            resolvers.AddRange(context.MemberMappers.Select(p => new MemberMapperSource(p, context)));
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
                    emitContext.ExecutionContext.ReportDiagnostic(
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