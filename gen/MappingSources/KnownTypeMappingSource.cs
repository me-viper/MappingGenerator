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
        private KnownMapper Mapper { get; }

        private bool _isInternal;

        public KnownTypeMappingSource(KnownMapper mapper, bool isInternal, MappingEmitContext context) : base(mapper, context)
        {
            _isInternal = isInternal;

            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
