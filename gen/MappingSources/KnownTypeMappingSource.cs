using System;

using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration.MappingSources
{
    internal class KnownTypeMappingSource : MemberMapperSource
    {
        private readonly bool _isInternal;

        public KnownTypeMappingSource(KnownMapperRef mapper, bool isInternal, MappingEmitContext context) : base(mapper, context)
        {
            _isInternal = isInternal;
        }

        public override MappingSpec? TryMap(MappingDestination entry)
        {
            var memberMapping = base.TryMap(entry);

            if (memberMapping == null)
                return null;

            var memberName = Context.MemberNamingManager.GetMemberName(Mapper);
            var result = new KnownTypeMappingSpec(memberName, Mapper, entry, _isInternal);
            result.MappingExpressions.AddRange(memberMapping.MappingExpressions);
            result.MappingStatements.AddRange(memberMapping.MappingStatements);

            return result;
        }
    }
}
