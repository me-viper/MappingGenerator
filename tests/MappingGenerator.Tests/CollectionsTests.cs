using Xunit;

using MappingGenerator.Tests.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Talk2Bits.MappingGenerator.Abstractions;

namespace MappingGenerator.Tests.Collections
{
    public class CollectionsTests
    {
        // TODO: ICollection write-only.

        [Theory]
        [MemberData(nameof(ConstructorMappingTestCases))]
        public void ConstructorMapping(ICollectionTestCase testCase)
        {
            testCase.TestMapping();
        }

        [Theory]
        [MemberData(nameof(ProppertyMappingTestCases))]
        public void ProppertyMapping(ICollectionTestCase testCase)
        {
            testCase.TestMapping();
        }

        [Theory]
        [MemberData(nameof(InitProppertiesMappingTestCases))]
        public void InitProppertyMapping(ICollectionTestCase testCase)
        {
            testCase.TestMapping();
        }

        public static IEnumerable<object> ProppertyMappingTestCases = new List<object>
        {
            IEnurableToDestinationIEnumerable(static () => new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new Mapper()),
            IEnurableToDestinationIEnumerable(static () => new int[] { 1, 2, 3 }, new long[] { 1, 2, 3 }, new Mapper()),
            IEnurableToDestinationIEnumerable(static () => new long[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new Mapper()),
            IEnurableToDestinationIEnumerable(static () => new A[] { new B() }, new B[] { new B() }, new Mapper()),
            IEnurableToDestinationIEnumerable(static () => new B[] { new B() }, new A[] { new B() }, new Mapper()),
        }.Select(p => new object[] { p });

        public static IEnumerable<object> ConstructorMappingTestCases = new List<object>
        {
            IEnurableToDestinationConstructorIEnumerable(static () => new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new Mapper()),
            IEnurableToDestinationConstructorIEnumerable(static () => new int[] { 1, 2, 3 }, new long[] { 1, 2, 3 }, new Mapper()),
            IEnurableToDestinationConstructorIEnumerable(static () => new long[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new Mapper()),
            IEnurableToDestinationConstructorIEnumerable(static () => new A[] { new B() }, new B[] { new B() }, new Mapper()),
            IEnurableToDestinationConstructorIEnumerable(static () => new B[] { new B() }, new A[] { new B() }, new Mapper()),
        }.Select(p => new object[] { p });
        
        public static IEnumerable<object> InitProppertiesMappingTestCases = new List<object>
        {
            IEnurableToDestinationInitIEnumerable(static () => new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new Mapper()),
            IEnurableToDestinationInitIEnumerable(static () => new int[] { 1, 2, 3 }, new long[] { 1, 2, 3 }, new Mapper()),
            IEnurableToDestinationInitIEnumerable(static () => new long[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new Mapper()),
            IEnurableToDestinationInitIEnumerable(static () => new A[] { new B() }, new B[] { new B() }, new Mapper()),
            IEnurableToDestinationInitIEnumerable(static () => new B[] { new B() }, new A[] { new B() }, new Mapper()),
        }.Select(p => new object[] { p });

        private static CollectionTestCase<IEnumerable<TIn>, Destination<IEnumerable<TOut>>, IEnumerable<TOut>>
            IEnurableToDestinationIEnumerable<TIn, TOut>(
                Func<IEnumerable<TIn>> source,
                IEnumerable<TOut> expected,
                object mapper)
        {
            return new CollectionTestCase<IEnumerable<TIn>, Destination<IEnumerable<TOut>>, IEnumerable<TOut>>(
                source,
                expected,
                (IMapper<Source<IEnumerable<TIn>>, Destination<IEnumerable<TOut>>>)mapper
                );
        }

        private static CollectionTestCase<IEnumerable<TIn>, DestinationConstructor<IEnumerable<TOut>>, IEnumerable<TOut>>
            IEnurableToDestinationConstructorIEnumerable<TIn, TOut>(
                Func<IEnumerable<TIn>> source,
                IEnumerable<TOut> expected,
                object mapper)
        {
            return new CollectionTestCase<IEnumerable<TIn>, DestinationConstructor<IEnumerable<TOut>>, IEnumerable<TOut>>(
                source,
                expected,
                (IMapper<Source<IEnumerable<TIn>>, DestinationConstructor<IEnumerable<TOut>>>)mapper
                );
        }
        
        private static CollectionTestCase<IEnumerable<TIn>, DestinationInitOnly<IEnumerable<TOut>>, IEnumerable<TOut>>
            IEnurableToDestinationInitIEnumerable<TIn, TOut>(
                Func<IEnumerable<TIn>> source,
                IEnumerable<TOut> expected,
                object mapper)
        {
            return new CollectionTestCase<IEnumerable<TIn>, DestinationInitOnly<IEnumerable<TOut>>, IEnumerable<TOut>>(
                source,
                expected,
                (IMapper<Source<IEnumerable<TIn>>, DestinationInitOnly<IEnumerable<TOut>>>)mapper
                );
        }

        [Fact]
        public void List()
        {
        }

        [Fact]
        public void ListWithInitOnly()
        {
        }
    }

    public record A 
    {
        public int P { get; } = 10;
    }

    public record B : A { }

    // Properties.
    [MappingGenerator(typeof(Source<IEnumerable<int>>), typeof(Destination<IEnumerable<int>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<int>>), typeof(Destination<IEnumerable<long>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<long>>), typeof(Destination<IEnumerable<int>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<B>>), typeof(Destination<IEnumerable<A>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<A>>), typeof(Destination<IEnumerable<B>>), ImplementationType = ImplementationType.Explicit)]
    // Constructor.
    [MappingGenerator(typeof(Source<IEnumerable<int>>), typeof(DestinationConstructor<IEnumerable<int>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<int>>), typeof(DestinationConstructor<IEnumerable<long>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<long>>), typeof(DestinationConstructor<IEnumerable<int>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<B>>), typeof(DestinationConstructor<IEnumerable<A>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<A>>), typeof(DestinationConstructor<IEnumerable<B>>), ImplementationType = ImplementationType.Explicit)]
    // InitOnly.
    [MappingGenerator(typeof(Source<IEnumerable<int>>), typeof(DestinationInitOnly<IEnumerable<int>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<int>>), typeof(DestinationInitOnly<IEnumerable<long>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<long>>), typeof(DestinationInitOnly<IEnumerable<int>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<B>>), typeof(DestinationInitOnly<IEnumerable<A>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<A>>), typeof(DestinationInitOnly<IEnumerable<B>>), ImplementationType = ImplementationType.Explicit)]
    public partial class Mapper
    {
    }
}
