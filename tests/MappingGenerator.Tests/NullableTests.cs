using MappingGenerator.Tests.Common;

using System;

using Talk2Bits.MappingGenerator.Abstractions;

using Xunit;

namespace MappingGenerator.Tests.Nullable
{
    public class NullableTests
    {
        [Fact]
        public void NullableSource()
        {
            var source = new Source<string?>();
            var mapper = new NonNullableMapper(new InnerMapper());

            Assert.Throws<SourceMemberNullException>(() => mapper.Map(source));
        }

        [Fact]
        public void NullableSourceAndDestination()
        {
            var source = new Source<string?>();
            var expected = new Destination<string?>();
            var mapper = new NullableMapper(new InnerMapper());
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void NullableSourceNested()
        {
            var source = new Source<SourceInner?>();
            var mapper = new NonNullableMapper(new InnerMapper());

            Assert.Throws<SourceMemberNullException>(() => mapper.Map(source));
        }

        [Fact]
        public void NullableSourceAndDestinationNested()
        {
            var source = new Source<SourceInner?>();
            var expected = new Destination<DestinationInner?>();
            var mapper = new NullableMapper(new InnerMapper());
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }
    }

    [MappingGenerator(typeof(Source<SourceInner?>), typeof(Destination<DestinationInner>))]
    [MappingGenerator(typeof(Source<string?>), typeof(Destination<string>))]
    public partial class NonNullableMapper
    { }

    [MappingGenerator(typeof(Source<SourceInner?>), typeof(Destination<DestinationInner?>))]
    [MappingGenerator(typeof(Source<string?>), typeof(Destination<string?>))]
    public partial class NullableMapper
    { }
}
