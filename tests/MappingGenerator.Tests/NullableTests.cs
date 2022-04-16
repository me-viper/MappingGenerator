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

            Assert.Throws<MappingNullException>(() => mapper.Map(source));
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

            Assert.Throws<MappingNullException>(() => mapper.Map(source));
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

        [Fact]
        public void CustomNullableSource()
        {
            var source = new Source<SourceInner?>();
            var expected = new Destination<DestinationInner> { Value = new() { InnerNumber = 100, InnerText = "Default" } };
            var mapper = new CustomNullableSourceMapper();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CustomNullableDestination()
        {
            var source = new Source<SourceInner?>();
            var mapper = new CustomNullableDestinationMapper();
            Assert.Throws<MappingNullException>(() => mapper.Map(source));
        }
    }

    [MappingGenerator(typeof(Source<SourceInner?>), typeof(Destination<DestinationInner>))]
    public partial class CustomNullableDestinationMapper
    {
        private IMapper<SourceInner?, DestinationInner?> _nullToDefaultMapper = new NullToDefaultMapper();

        private class NullToDefaultMapper : IMapper<SourceInner?, DestinationInner?>
        {
            public DestinationInner? Map(SourceInner? source)
            {
                if (source == null)
                    return null;

                return new DestinationInner { InnerNumber = source.InnerNumber, InnerText = source.InnerText };
            }
        }
    }
    
    [MappingGenerator(typeof(Source<SourceInner?>), typeof(Destination<DestinationInner>))]
    public partial class CustomNullableSourceMapper
    {
        private IMapper<SourceInner?, DestinationInner> _nullToDefaultMapper = new NullToDefaultMapper();

        private class NullToDefaultMapper : IMapper<SourceInner?, DestinationInner>
        {
            public DestinationInner Map(SourceInner? source)
            {
                if (source == null)
                    return new DestinationInner { InnerNumber = 100, InnerText = "Default" };

                return new DestinationInner { InnerNumber = source.InnerNumber, InnerText = source.InnerText };
            }
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
