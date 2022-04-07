using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using MappingGenerator.Tests.BasicTypesMapping;
using MappingGenerator.Tests.Common;

using Talk2Bits.MappingGenerator.Abstractions;
using Talk2Bits.MappingGenerator.DependencyInjection;

using Xunit;

namespace MappingGenerator.Tests.DependencyInjection
{
    public class AggregatedMapperTests
    {
        [Fact]
        public void Basic()
        {
            var mappers = new IAbstractMapper[] { new BasicIntMapper(), new BasicLongMapper() };
            var agg = new AggregatedMapper(mappers);
            var source = new Source<int> { Value = 100 };
            _ = agg.Map<Source<int>, Destination<int>>(source);
        }

        [Fact]
        public void EnumerableToEnumerable()
        {
            var mappers = new IAbstractMapper[] { new BasicIntMapper() };
            var agg = new AggregatedMapper(mappers);
            _ = agg.Map<IEnumerable<Source<int>>, IEnumerable<Destination<int>>>(new Source<int>[] {});
        }
        
        [Fact]
        public void EnumerableToCollection()
        {
            var mappers = new IAbstractMapper[] { new BasicIntMapper() };
            var agg = new AggregatedMapper(mappers);
            _ = agg.Map<IEnumerable<Source<int>>, Collection<Destination<int>>>(new Source<int>[] {});
        }
        
        [Fact]
        public void EnumerableToHashSet()
        {
            var mappers = new IAbstractMapper[] { new BasicIntMapper() };
            var agg = new AggregatedMapper(mappers);
            _ = agg.Map<IEnumerable<Source<int>>, HashSet<Destination<int>>>(new Source<int>[] {});
        }
        
        [Fact]
        public void EnumerableToList()
        {
            var mappers = new IAbstractMapper[] { new BasicIntMapper() };
            var agg = new AggregatedMapper(mappers);
            _ = agg.Map<IEnumerable<Source<int>>, List<Destination<int>>>(new Source<int>[] {});
        }
        
        [Fact]
        public void EnumerableToIReadOnlyCollection()
        {
            var mappers = new IAbstractMapper[] { new BasicIntMapper() };
            var agg = new AggregatedMapper(mappers);
            _ = agg.Map<IEnumerable<Source<int>>, IReadOnlyCollection<Destination<int>>>(new Source<int>[] {});
        }
        
        [Fact]
        public void EnumerableToArray()
        {
            var mappers = new IAbstractMapper[] { new BasicIntMapper() };
            var agg = new AggregatedMapper(mappers);
            _ = agg.Map<IEnumerable<Source<int>>, Destination<int>[]>(new Source<int>[] {});
        }

        [Fact]
        public void ArrayToArray()
        {
            var mappers = new IAbstractMapper[] { new BasicIntMapper() };
            var agg = new AggregatedMapper(mappers);
            _ = agg.Map<Source<int>[], Destination<int>[]>(new Source<int>[] {});
        }

        [Fact]
        public void Covariant()
        {
            var mappers = new IAbstractMapper[] { new Mapper() };
            var agg = new AggregatedMapper(mappers);
            var source = new A();
            _ = agg.Map<A, C>(source);
        }

        [Fact]
        public void Contravariant()
        {
            var mappers = new IAbstractMapper[] { new Mapper() };
            var agg = new AggregatedMapper(mappers);
            var source = new B();
            _ = agg.Map<B, D>(source);
        }
    }

    [MappingGenerator(typeof(A), typeof(D))]
    public partial class Mapper
    { }
}
