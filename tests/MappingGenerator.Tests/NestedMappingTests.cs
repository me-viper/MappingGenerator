using Xunit;

using MappingGenerator.Abstractions;
using MappingGenerator.Tests.Common;

namespace MappingGenerator.Tests.NestedMapping
{
    public class NestedMappingTests
    {
        [Fact]
        public void Test()
        {
            var innerMapper = new InnerMapper();
            var mapper = new Mapper(innerMapper);

            var source = new Source<SourceInner, SourceInner>
            {
                Value = new()
                {
                    InnerNumber = 10,
                    InnerText = "T",
                },
                Value2 = new()
                {
                    InnerNumber = 11,
                    InnerText = "T1",
                },
            };

            var expected = new Destination<DestinationInner, DestinationInner>
            {
                Value = new()
                {
                    InnerNumber = 10,
                    InnerText = "T",
                },
                Value2 = new()
                {
                    InnerNumber = 11,
                    InnerText = "T1",
                },
            };

            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }
    }

    [MappingGenerator(typeof(Source<SourceInner, SourceInner>), typeof(Destination<DestinationInner, DestinationInner>))]
    public partial class Mapper
    { }
}