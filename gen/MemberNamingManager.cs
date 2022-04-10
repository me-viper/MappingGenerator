using System;
using System.Collections.Generic;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class MemberNamingManager
    {
        private readonly Dictionary<string, KnownMapperRef> _usedNames;

        public MemberNamingManager(IDictionary<string, KnownMapperRef> mappers)
        {
            _usedNames = new Dictionary<string, KnownMapperRef>(mappers);
        }

        public string GetMemberName(KnownMapperRef mapper)
        {
            var memberName = mapper.MemberName;
            
            string MemberNameInner(int? suffix = null)
            {
                var name = memberName + (suffix?.ToString() ?? string.Empty);

                if (!_usedNames.TryGetValue(name, out var mp))
                {
                    _usedNames.Add(name, mapper);
                    return name;
                }

                if (mp.Equals(mapper))
                    return name;

                return MemberNameInner((suffix ?? 0) + 1);
            }

            return MemberNameInner();
        }
    }
}
