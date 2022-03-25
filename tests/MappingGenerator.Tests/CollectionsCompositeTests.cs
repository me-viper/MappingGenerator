using Xunit;

using MappingGenerator.Abstractions;
using MappingGenerator.Tests.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MappingGenerator.Tests.CollectionsComposite
{
    using Source = Source<int, IEnumerable<SourceInner>>;
    using DestinationWithCollection = DestinationWithCollection<int, List<DestinationInner>, DestinationInner>;
    using DestinationConstructor = DestinationConstructor<int, List<DestinationInner>>;
    using DestinationInitOnly = DestinationInitOnly<int, List<DestinationInner>>;

    public class CollectionsCompositeTests
    {
        [Fact]
        public void ListWithConstructor()
        {
            var numbers = new[] { 1, 2, 3, 4, 5 };

            var source = new Source
            {
                Value = 1,
                Value2 = numbers.Select(p => new SourceInner { InnerText = $"T{p}", InnerNumber = p }).ToList()
            };

            var inners = numbers.Select(p => new DestinationInner { InnerText = $"T{p}", InnerNumber = p }).ToList();
            var expected = new DestinationConstructor(inners) { Value = 1 };

            var mapper = new CollectionsConstructorMapper(new InnerMapper());
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
                Value2 = numbers.Select(p => new SourceInner { InnerText = $"T{p}", InnerNumber = p }).ToList()
            };

            var inners = numbers.Select(p => new DestinationInner { InnerText = $"T{p}", InnerNumber = p }).ToList();
            var expected = new DestinationWithCollection { Value = 1 };
            expected.Value2.AddRange(inners);
            var mapper = new CollectionsMapper(new InnerMapper());
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
                Value2 = numbers.Select(p => new SourceInner { InnerText = $"T{p}", InnerNumber = p }).ToList()
            };

            var inners = numbers.Select(p => new DestinationInner { InnerText = $"T{p}", InnerNumber = p }).ToList();
            var expected = new DestinationInitOnly { Value = 1, Value2 = inners };
            var mapper = new CollectionsInitMapper(new InnerMapper());
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
