using MappingGenerator.Abstractions;
using MappingGenerator.Tests.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace MappingGenerator.Tests.InitOnlyMapping
{
    public class InitOnlyMappingTests
    {
        [Fact]
        public void InitOnlyProperties()
        {
            var source = new Source<string, int> { Value = "Text", Value2 = 100 };
            var expected = new DestinationInitOnly<string, int> { Value = "Text", Value2 = 100 };

            var mapper = new InitOnly();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void InitOnlyPropertiesWithCustomMapping()
        {
            var source = new Source<string, int> { Value = "Text", Value2 = 100 };
            var expected = new DestinationInitOnly<string, int> { Value = "CustomText", Value2 = 100 };

            var mapper = new InitOnlyMapperCustomMapping();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void InitOnlyNested()
        {
            var source = new Source<string, SourceInner> { Value = "Text", Value2 = SourceInner.Default };
            var expected = new DestinationInitOnly<string, DestinationInner> { Value = "Text", Value2 = DestinationInner.Default };

            var nested = new InnerMapper();
            var mapper = new InitOnlyNested(nested);
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }
    }

    [MappingGenerator(typeof(Source<string, int>), typeof(DestinationInitOnly<string, int>))]
    public partial class InitOnly
    { }

    [MappingGenerator(typeof(Source<string, int>), typeof(DestinationInitOnly<string, int>))]
    public partial class InitOnlyMapperCustomMapping
    {
        public string? MapValue(Source<string, int> source)
        {
            return $"Custom{source.Value}";
        }
    }

    [MappingGenerator(typeof(Source<string, SourceInner>), typeof(DestinationInitOnly<string, DestinationInner>))]
    public partial class InitOnlyNested
    {
    }
}
