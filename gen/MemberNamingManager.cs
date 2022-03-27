using System;
using System.Collections.Generic;
using System.Text;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class MemberNamingManager
    {
        private Dictionary<string, KnownMapper> _usedNames = new();

        public string GetMemberName(KnownMapper mapper)
        {
            string MemberNameInner(KnownMapper m, int? suffix = null)
            {
                var name = char.ToLower(mapper.Name[0]) + mapper.Name.Substring(1) + (suffix?.ToString() ?? string.Empty);

                if (!_usedNames.TryGetValue(name, out var mp))
                {
                    _usedNames.Add(name, mapper);
                    return name;
                }

                if (mp.Equals(mapper))
                    return name;

                return MemberNameInner(m, (suffix ?? 0) + 1);
            }

            return MemberNameInner(mapper);
        }
    }
}
