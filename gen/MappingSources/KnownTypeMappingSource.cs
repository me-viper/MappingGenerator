using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Talk2Bits.MappingGenerator.SourceGeneration.Spec;

namespace Talk2Bits.MappingGenerator.SourceGeneration.MappingSources
{
    internal class KnownTypeMappingSource : MemberMapperSource
    {
        private readonly KnownMapper _mapper;

        private readonly bool _isInternal;

        public KnownTypeMappingSource(KnownMapper mapper, bool isInternal, MappingEmitContext context) : base(mapper, context)
        {
            _isInternal = isInternal;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public override MappingSpec? TryMap(MappingDestination entry)
        {
            var memberMapping = base.TryMap(entry);

            if (memberMapping == null)
                return null;

            var memberName = Context.MemberNamingManager.GetMemberName(_mapper);
            var result = new KnownTypeMappingSpec(memberName, _mapper, entry, _isInternal);
            result.MappingExpressions.AddRange(memberMapping.MappingExpressions);
            result.MappingStatements.AddRange(memberMapping.MappingStatements);

            return result;
        }
    }
}
