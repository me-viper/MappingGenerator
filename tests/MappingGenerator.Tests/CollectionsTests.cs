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
            IEnurableToDestinationIEnumerable(static () => new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new EnumerableMapper()),
            IEnurableToDestinationIEnumerable(static () => new int[] { 1, 2, 3 }, new long[] { 1, 2, 3 }, new EnumerableMapper()),
            IEnurableToDestinationIEnumerable(static () => new long[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new EnumerableMapper()),
            IEnurableToDestinationIEnumerable(static () => new A[] { new B() }, new B[] { new B() }, new EnumerableMapper()),
            IEnurableToDestinationIEnumerable(static () => new B[] { new B() }, new A[] { new B() }, new EnumerableMapper()),
            IEnurableToDestinationHashSet(static () => new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new HashSetMapper()),
            IEnurableToDestinationHashSet(static () => new int[] { 1, 2, 3 }, new long[] { 1, 2, 3 }, new HashSetMapper()),
            IEnurableToDestinationHashSet(static () => new long[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new HashSetMapper()),
            IEnurableToDestinationHashSet(static () => new A[] { new B() }, new B[] { new B() }, new HashSetMapper()),
            IEnurableToDestinationHashSet(static () => new B[] { new B() }, new A[] { new B() }, new HashSetMapper()),
            IEnurableToReadOnlyDestinationHashSet(static () => new A[] { new B() }, (B[]?)null, new HashSetMapper()),
        }.Select(p => new object[] { p });

        public static IEnumerable<object> ConstructorMappingTestCases = new List<object>
        {
            IEnurableToDestinationConstructorIEnumerable(static () => new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new EnumerableMapper()),
            IEnurableToDestinationConstructorIEnumerable(static () => new int[] { 1, 2, 3 }, new long[] { 1, 2, 3 }, new EnumerableMapper()),
            IEnurableToDestinationConstructorIEnumerable(static () => new long[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new EnumerableMapper()),
            IEnurableToDestinationConstructorIEnumerable(static () => new A[] { new B() }, new B[] { new B() }, new EnumerableMapper()),
            IEnurableToDestinationConstructorIEnumerable(static () => new B[] { new B() }, new A[] { new B() }, new EnumerableMapper()),
        }.Select(p => new object[] { p });
        
        public static IEnumerable<object> InitProppertiesMappingTestCases = new List<object>
        {
            IEnurableToDestinationInitIEnumerable(static () => new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new EnumerableMapper()),
            IEnurableToDestinationInitIEnumerable(static () => new int[] { 1, 2, 3 }, new long[] { 1, 2, 3 }, new EnumerableMapper()),
            IEnurableToDestinationInitIEnumerable(static () => new long[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new EnumerableMapper()),
            IEnurableToDestinationInitIEnumerable(static () => new A[] { new B() }, new B[] { new B() }, new EnumerableMapper()),
            IEnurableToDestinationInitIEnumerable(static () => new B[] { new B() }, new A[] { new B() }, new EnumerableMapper()),
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

       private static CollectionTestCase<IEnumerable<TIn>, Destination<HashSet<TOut>>, HashSet<TOut>>
            IEnurableToDestinationHashSet<TIn, TOut>(
                Func<IEnumerable<TIn>> source,
                IEnumerable<TOut> expected,
                object mapper)
        {
            return new CollectionTestCase<IEnumerable<TIn>, Destination<HashSet<TOut>>, HashSet<TOut>>(
                source,
                new HashSet<TOut>(expected),
                (IMapper<Source<IEnumerable<TIn>>, Destination<HashSet<TOut>>>)mapper
                );
        }

       private static CollectionTestCase<IEnumerable<TIn>, DestinationReadOnly<HashSet<TOut>>, HashSet<TOut>>
            IEnurableToReadOnlyDestinationHashSet<TIn, TOut>(
                Func<IEnumerable<TIn>> source,
                IEnumerable<TOut>? expected,
                object mapper)
        {
            return new CollectionTestCase<IEnumerable<TIn>, DestinationReadOnly<HashSet<TOut>>, HashSet<TOut>>(
                source,
                expected == null ? null : new HashSet<TOut>(expected),
                (IMapper<Source<IEnumerable<TIn>>, DestinationReadOnly<HashSet<TOut>>>)mapper
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
    }

    public record A 
    {
        public int P { get; } = 10;
    }

    public record B : A { }

    // Properties. IEnumerable.
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
    public partial class EnumerableMapper
    {
    }

    // Properties. HashSet.
    [MappingGenerator(typeof(Source<IEnumerable<int>>), typeof(Destination<HashSet<int>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<int>>), typeof(Destination<HashSet<long>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<long>>), typeof(Destination<HashSet<int>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<B>>), typeof(Destination<HashSet<A>>), ImplementationType = ImplementationType.Explicit)]
    [MappingGenerator(typeof(Source<IEnumerable<A>>), typeof(Destination<HashSet<B>>), ImplementationType = ImplementationType.Explicit)]
    // Readonly properties. HashSet.
    [MappingGenerator(typeof(Source<IEnumerable<A>>), typeof(DestinationReadOnly<HashSet<B>>), ImplementationType = ImplementationType.Explicit)]
    public partial class HashSetMapper
    { }
}
