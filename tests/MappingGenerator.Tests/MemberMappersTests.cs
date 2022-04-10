using System;

using Talk2Bits.MappingGenerator.Abstractions;

using Xunit;

namespace MappingGenerator.Tests.MemberMappers
{
    public class MemberMappersTests
    {
        [Fact]
        public void Run()
        {
            var source = new Source { Value1 = new A { Value = "A1" }, Value2 = new A { Value = "A2" } };
            var expected = new Destination { Value1 = new B { Value = "A1" }, Value2 = new C { Value = "A2" } };
            var mapper = new Mapper();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }
    }

    public record A { public string Value { get; set; } = default!; }
    public record B { public string Value { get; set; } = default!; }
    public record C { public string Value { get; set; } = default!; }

    public record Source
    {
        public A Value1 { get; set; } = default!;
        public A Value2 { get; set; } = default!;
    }

    public record Destination
    {
        public B Value1 { get; set; } = default!;
        public C Value2 { get; set; } = default!;
    }

    [MappingGenerator(typeof(Source), typeof(Destination))]
    public partial class Mapper
    {
        private IMapper<A, B> _fieldMapper = new A2B();

        public IMapper<A, C> PropertyMapper { get; } = new A2C();

        private class A2B : IMapper<A, B>
        {
            public B Map(A source)
            {
                return new B { Value = source.Value };
            }
        }
        
        private class A2C : IMapper<A, C>
        {
            public C Map(A source)
            {
                return new C { Value = source.Value };
            }
        }
    }
}
