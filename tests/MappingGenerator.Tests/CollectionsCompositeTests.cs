using Xunit;

using MappingGenerator.Tests.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Talk2Bits.MappingGenerator.Abstractions;


namespace MappingGenerator.Tests.CollectionsComposite
{
    using Source = Source<int, IEnumerable<SourceInner>>;
    using ListWithCollection = DestinationWithCollection<int, List<DestinationInner>, DestinationInner>;
    using ListWithConstructor = DestinationConstructor<int, List<DestinationInner>>;
    using ListWithInitOnly = DestinationInitOnly<int, List<DestinationInner>>;

    using EnumerableWithCollection = DestinationWithCollection<int, List<DestinationInner>, DestinationInner>;
    using EnumerableWithConstructor = DestinationConstructor<int, IEnumerable<DestinationInner>>;
    using EnumerableWithInitOnly = DestinationInitOnly<int, List<DestinationInner>>;

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
            var expected = new ListWithConstructor(inners) { Value = 1 };

            var mapper = new ListConstructorMapper(new InnerMapper());
            var result = mapper.Map(source);

            Assert.Equal(expected.Value, result.Value);
            Assert.Equal(expected.Value2, result.Value2);
        }
        
        [Fact]
        public void EnumerableWithConstructor()
        {
            var numbers = new[] { 1, 2, 3, 4, 5 };
            
            var source = new Source
            {
                Value = 1,
                Value2 = numbers.Select(p => new SourceInner { InnerText = $"T{p}", InnerNumber = p }).ToList()
            };

            var inners = numbers.Select(p => new DestinationInner { InnerText = $"T{p}", InnerNumber = p }).ToList();
            var expected = new EnumerableWithConstructor(inners) { Value = 1 };

            var mapper = new EnumerableConstructorMapper(new InnerMapper());
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
            var expected = new ListWithCollection { Value = 1 };
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
            var expected = new ListWithInitOnly { Value = 1, Value2 = inners };
            var mapper = new CollectionsInitMapper(new InnerMapper());
            var result = mapper.Map(source);

            Assert.Equal(expected.Value, result.Value);
            Assert.Equal(expected.Value2, result.Value2);
        }
    }

    [MappingGenerator(typeof(Source), typeof(ListWithConstructor))]
    public partial class ListConstructorMapper
    { }
    
    [MappingGenerator(typeof(Source), typeof(EnumerableWithConstructor))]
    public partial class EnumerableConstructorMapper
    { }

    [MappingGenerator(typeof(Source), typeof(ListWithCollection))]
    public partial class CollectionsMapper
    { }

    [MappingGenerator(typeof(Source), typeof(ListWithInitOnly))]
    public partial class CollectionsInitMapper
    { }
}
