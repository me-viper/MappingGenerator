using System;

using Talk2Bits.MappingGenerator.Abstractions;

using Xunit;

namespace MappingGenerator.Tests.ValueTypes
{
    public class ValueTypesTests
    {
        [Fact]
        public void Map()
        {
            var source = new Source { Number = 100 };
            var expected = new Destination { Number = 100, AfterMap = 1 };
            var mapper = new Mapper();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }
    }

    public struct Source
    {
        public int Number { get; set; }
    }

    public struct Destination
    {
        public int Number { get; set; }

        public int AfterMap { get; set; }
    }

    [MappingGenerator(typeof(Source), typeof(Destination))]
    [MappingGeneratorPropertyIgnore(nameof(Destination.AfterMap))]
    public partial class Mapper
    {
        partial void AfterMap(Source source, ref Destination result)
        {
            result.AfterMap = 1;
        }
    }
}
