using MappingGenerator.Tests.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Talk2Bits.MappingGenerator.Abstractions;

using Xunit;

namespace MappingGenerator.Tests.MultiMapping
{
    public class MultiMappingTests
    {
        [Fact]
        public void MultiMapping()
        {
            var mapper = new Mapper();

            var sourceInt = new Source<int> { Value = 10 };
            var expectedInt = new Destination<int> { Value = 10 };
            var resultInt = mapper.Map(sourceInt);

            Assert.Equal(expectedInt, resultInt);

            var sourceLong = new Source<long> { Value = 100 };
            var expectedLong = new Destination<long> { Value = 100 };
            var resultLong = mapper.Map(sourceLong);

            Assert.Equal(expectedLong, resultLong);

            var sourceString = new Source<string> { Value = "1000" };
            var expectedString = new Destination<string> { Value = "1000" };
            var resultString = mapper.Map(sourceString);

            Assert.Equal(expectedString, resultString);

        }

        [Fact]
        public void MuliMappingInternal()
        {
            var source = new Source<SourceInner> { Value = new() { InnerNumber = 100, InnerText = "Text" } };
            var expected = new Destination<DestinationInner> { Value = new() { InnerNumber = 100, InnerText = "Text" } };
            var mapper = new InternalMapper();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void MuliMappingExternal()
        {
            var source = new Source<Source<int>> { Value = new() { Value = 100 } };
            var expected = new Destination<Destination<int>> { Value = new() { Value = 100 } };

            var externalMapper = new Mapper();
            var mapper = new ExternalMapper(externalMapper, externalMapper, externalMapper);
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }
    }

    [MappingGenerator(typeof(Source<int>), typeof(Destination<int>))]
    [MappingGenerator(typeof(Source<long>), typeof(Destination<long>))]
    [MappingGenerator(typeof(Source<string>), typeof(Destination<string>))]
    public partial class Mapper
    { }

    [MappingGenerator(typeof(SourceInner), typeof(DestinationInner))]
    [MappingGenerator(typeof(Source<SourceInner>), typeof(Destination<DestinationInner>))]
    public partial class InternalMapper
    { }

    [MappingGenerator(typeof(Source<Source<int>>), typeof(Destination<Destination<int>>))]
    [MappingGenerator(typeof(Source<Source<long>>), typeof(Destination<Destination<long>>))]
    [MappingGenerator(typeof(Source<Source<string>>), typeof(Destination<Destination<string>>))]
    public partial class ExternalMapper
    { }
}
