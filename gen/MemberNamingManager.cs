using System;
using System.Collections.Generic;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class MemberNamingManager
    {
        private readonly Dictionary<string, KnownMapper> _usedNames = new();

        public string GetMemberName(KnownMapper mapper)
        {
            var mapperName = $"{char.ToLower(mapper.Name[0])}{mapper.Name.Substring(1)}";
            
            string MemberNameInner(int? suffix = null)
            {
                var name = mapperName + (suffix?.ToString() ?? string.Empty);

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
