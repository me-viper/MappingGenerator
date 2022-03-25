using Xunit;

using MappingGenerator.Abstractions;
using MappingGenerator.Tests.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MappingGenerator.Tests.Collections
{
    using Source = Source<int, IEnumerable<int>>;
    using DestinationWithCollection = DestinationWithCollection<int, List<int>>;
    using DestinationConstructor = DestinationConstructor<int, List<int>>;
    using DestinationInitOnly = DestinationInitOnly<int, List<int>>;

    public class CollectionsTests
    {
        [Fact]
        public void ListWithConstructor()
        {
            var numbers = new[] { 1, 2, 3, 4, 5 };

            var source = new Source
            {
                Value = 1,
                Value2 = numbers.ToList()
            };

            var expected = new DestinationConstructor(numbers.ToList()) { Value = 1 };
            var mapper = new CollectionsConstructorMapper();
            var result = mapper.Map(source);

            Assert.Equal(expected.Value, result.Value);
            Assert.Equal(expected.Value2, result.Value2);
        }

        [Fact]
        public void List()
        {
            var numbers = new[] { 1, 2, 3, 4, 5 };

            var source = new Source
            {
                Value = 1,
                Value2 = numbers.ToList()
            };

            var expected = new DestinationWithCollection { Value = 1 };
            expected.Value2.AddRange(numbers);
            var mapper = new CollectionsMapper();
            var result = mapper.Map(source);

            Assert.Equal(expected.Value, result.Value);
            Assert.Equal(expected.Value2, result.Value2);
        }

        [Fact]
        public void ListWithInitOnly()
        {
            var numbers = new[] { 1, 2, 3, 4, 5 };

            var source = new Source
            {
                Value = 1,
                Value2 = numbers.ToList()
            };

            var expected = new DestinationInitOnly { Value = 1, Value2 = numbers.ToList() };
            var mapper = new CollectionsInitMapper();
            var result = mapper.Map(source);

            Assert.Equal(expected.Value, result.Value);
            Assert.Equal(expected.Value2, result.Value2);
        }
    }

    [MappingGenerator(typeof(Source), typeof(DestinationConstructor))]
    public partial class CollectionsConstructorMapper
    { }

    [MappingGenerator(typeof(Source), typeof(DestinationWithCollection))]
    public partial class CollectionsMapper
    { }

    [MappingGenerator(typeof(Source), typeof(DestinationInitOnly))]
    public partial class CollectionsInitMapper
    { }
}
