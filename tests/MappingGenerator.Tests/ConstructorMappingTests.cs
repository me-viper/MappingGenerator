using MappingGenerator.Abstractions;
using MappingGenerator.Tests.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace MappingGenerator.Tests.ConstructorMapping
{
    public class ConstructorMappingTests
    {
        [Fact]
        public void InitOnlyProperties()
        {
            var source = new Source<string, int> { Value = "Text", Value2 = 100 };
            var expected = new DestinationConstructor<string, int>(100) { Value = "Text" };

            var mapper = new ConstructorOnly();
            var result = mapper.Map(source);

            Assert.Equal<DestinationConstructor<string, int>>(expected, result);
        }

        [Fact]
        public void InitOnlyPropertiesWithCustomMapping()
        {
            var source = new Source<string, int> { Value = "Text", Value2 = 100 };
            var expected = new DestinationConstructor<string, int>(100) { Value = "CustomText" };

            var mapper = new ConstructorOnlyMapperCustomMapping();
            var result = mapper.Map(source);

            Assert.Equal<DestinationConstructor<string, int>>(expected, result);
        }

        [Fact]
        public void InitOnlyNested()
        {
            var source = new Source<string, SourceInner> { Value = "Text", Value2 = SourceInner.Default };
            var expected = new DestinationConstructor<string, DestinationInner>(DestinationInner.Default) { Value = "Text" };

            var nested = new InnerMapper();
            var mapper = new ConstructorOnlyNested(nested);
            var result = mapper.Map(source);

            Assert.Equal<DestinationConstructor<string, DestinationInner>>(expected, result);
        }
    }

    [MappingGenerator(typeof(Source<string, int>), typeof(DestinationConstructor<string, int>))]
    public partial class ConstructorOnly
    { }

    [MappingGenerator(typeof(Source<string, int>), typeof(DestinationConstructor<string, int>))]
    public partial class ConstructorOnlyMapperCustomMapping
    {
        public string? MapValue(Source<string, int> source)
        {
            return $"Custom{source.Value}";
        }
    }

    [MappingGenerator(typeof(Source<string, SourceInner>), typeof(DestinationConstructor<string, DestinationInner>))]
    public partial class ConstructorOnlyNested
    {
    }
}
