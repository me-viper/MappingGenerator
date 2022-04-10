using System;

using Talk2Bits.MappingGenerator.Abstractions;

using Xunit;

namespace MappingGenerator.Tests.Inheritance
{
    public class InheritanceTests
    {
        [Fact]
        public void Abstract()
        {
            var source = new Source { Value = "Text" };
            AbstractDestination expected = new Destination { Value = "Text" };
            var mapper = new Mapper();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }
    }

    public record Source
    {
        public string Value { get; set; } = default!;
    }

    public abstract record AbstractDestination
    {
        public string Value { get; set; } = default!;
    }

    public record Destination : AbstractDestination
    { }

    [MappingGenerator(typeof(Source), typeof(AbstractDestination))]
    public partial class Mapper
    {
        private AbstractDestination CreateDestination(Source source)
        {
            return new Destination();
        }
    }
}
