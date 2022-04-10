using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class EmitContext
    {
        public INamedTypeSymbol MapperType { get; }

        public IGeneratorContext ExecutionContext { get; }

        public KnownTypeSymbols KnownTypes { get; }

        public CollectionClassifier CollectionClassifier { get; }

        public MemberNamingManager MemberNamingManager { get; private set; } = default!;

        public IReadOnlyCollection<KnownMapperRef> MemberMappers { get; private set; } = default!;
        
        public IReadOnlyCollection<KnownMapper> InternalMappers { get; private set; } = default!;
        
        public IReadOnlyCollection<KnownMapper> KnownMappers { get; private set; } = default!;

        protected EmitContext(INamedTypeSymbol mapperType, IGeneratorContext executionContext)
        {
            MapperType = mapperType ?? throw new ArgumentNullException(nameof(mapperType));
            ExecutionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
            KnownTypes = new KnownTypeSymbols(executionContext.Compilation);
            CollectionClassifier = new CollectionClassifier(KnownTypes);
        }

        public static EmitContext Build(
            INamedTypeSymbol mapperType, 
            IGeneratorContext executionContext,
            IEnumerable<KnownMapper> internalMappers,
            IEnumerable<KnownMapper> knownMappers)
        {
            if (mapperType == null)
                throw new ArgumentNullException(nameof(mapperType));
            
            if (executionContext == null)
                throw new ArgumentNullException(nameof(executionContext));
            
            if (internalMappers == null)
                throw new ArgumentNullException(nameof(internalMappers));

            if (knownMappers == null)
                throw new ArgumentNullException(nameof(knownMappers));

            var result = new EmitContext(mapperType, executionContext);
            result.InternalMappers = new List<KnownMapper>(internalMappers);
            result.KnownMappers = new List<KnownMapper>(knownMappers);

            SetMemberMappers(result);

            result.MemberNamingManager = new(result.MemberMappers.ToDictionary(p => p.MemberName));

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

        private static void SetMemberMappers(EmitContext context)
        {
            var memberMappers = new List<KnownMapperRef>();

            void GetMemberMappers(INamedTypeSymbol? type)
            {
                if (type == null)
                    return;

                var members = type.GetMembers();

                var fields = members
                    .OfType<IFieldSymbol>()
                    .Where(p => p.Type.OriginalDefinition.Equals(context.KnownTypes.IMapper, SymbolEqualityComparer.Default))
                    .Where(p => p.CanBeReferencedByName)
                    .Where(p => context.ExecutionContext.Compilation.IsSymbolAccessibleWithin(p, context.MapperType));

                foreach (var field in fields)
                {
                    var source = (INamedTypeSymbol)((INamedTypeSymbol)field.Type).TypeArguments[0];
                    var dest = (INamedTypeSymbol)((INamedTypeSymbol)field.Type).TypeArguments[1];
                    var kmr = new KnownMapperRef(context.MapperType, source!, dest!, field.Name);
                    memberMappers.Add(kmr);
                }

                var props = members
                    .OfType<IPropertySymbol>()
                    .Where(p => p.Type.OriginalDefinition.Equals(context.KnownTypes.IMapper, SymbolEqualityComparer.Default))
                    .Where(p => p.CanBeReferencedByName)
                    .Where(p => p.GetMethod != null)
                    .Where(p => context.ExecutionContext.Compilation.IsSymbolAccessibleWithin(p.GetMethod!, context.MapperType));

                foreach (var prop in props)
                {
                    var source = (INamedTypeSymbol)((INamedTypeSymbol)prop.Type).TypeArguments[0];
                    var dest = (INamedTypeSymbol)((INamedTypeSymbol)prop.Type).TypeArguments[1];
                    var kmr = new KnownMapperRef(context.MapperType, source!, dest!, prop.Name);
                    memberMappers.Add(kmr);
                }

                GetMemberMappers(type.BaseType);
            }

            GetMemberMappers(context.MapperType);

            context.MemberMappers = memberMappers;
        }
    }
}