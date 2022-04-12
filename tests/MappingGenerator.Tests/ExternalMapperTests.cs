using MappingGenerator.ExternalMappers;
using MappingGenerator.Tests.Common;

using System;

using Talk2Bits.MappingGenerator.Abstractions;

using Xunit;

[assembly: MappingGeneratorIncludeAssembly("MappingGenerator.ExternalMappers")]

namespace MappingGenerator.Tests.ExternalMapper
{
    public class ExternalMapperTests
    {
        [Fact]
        public void ExternalMapper()
        {
            var source = new Source<ExternalSource> { Value = new ExternalSource { Text = "A" } };
            var expected = new Destination<ExternalDestination> 
            { 
                Value = new ExternalDestination { Text = "A", ExternalText = "ExternalA" } 
            };
            var mapper = new Mapper(new MappingGenerator.ExternalMappers.ExternalMapper());
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }
    }

    [MappingGenerator(typeof(Source<ExternalSource>), typeof(Destination<ExternalDestination>))]
    public partial class Mapper
    { }
}
