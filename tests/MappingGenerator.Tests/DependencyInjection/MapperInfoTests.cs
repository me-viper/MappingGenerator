using MappingGenerator.Tests.Common;

using System;
using System.Collections.Generic;

using Talk2Bits.MappingGenerator.DependencyInjection;

using Xunit;

namespace MappingGenerator.Tests.DependencyInjection
{
    public class MapperInfoTests
    {
        [Theory]
        [MemberData(nameof(Cases))]
        internal void Test(MapperInfo mapper, MapperInfo test, bool canMap)
        {
            var result = mapper.CanMap(test);
            Assert.Equal(canMap, result);
        }

        public static IEnumerable<object[]> Cases = new List<object[]>
        {
            new object[] { new MapperInfo(typeof(A), typeof(D)), new MapperInfo(typeof(A), typeof(C)), true },
            new object[] { new MapperInfo(typeof(A), typeof(D)), new MapperInfo(typeof(B), typeof(D)), true },
            new object[] { new MapperInfo(typeof(A), typeof(D)), new MapperInfo(typeof(B), typeof(C)), true },
            new object[] 
            { 
                new MapperInfo(typeof(Source<>), typeof(Destination<>)), 
                new MapperInfo(typeof(Source<A>), typeof(Destination<C>)), 
                true 
            },
        };
    }
}
