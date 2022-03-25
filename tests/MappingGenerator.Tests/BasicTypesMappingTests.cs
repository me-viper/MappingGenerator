using Xunit;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using MappingGenerator.Abstractions;
using MappingGenerator.Tests.Common;

namespace MappingGenerator.Tests.BasicTypesMapping
{
    public class BasicTypesMappingTests
    {
        [Theory]
        [MemberData(nameof(SimpleTypes))]
        public void BasicTypes(Type type, object value)
        {
            var method = typeof(BasicTypesMappingTests).GetMethod(
                nameof(RunTest),
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic
                );

            var generic = method.MakeGenericMethod(type);
            generic.Invoke(this, new[] { value });
        }

        [Fact]
        public void IEnumerableToList()
        {
            var ar = new int[] { 1, 2, 3 };
            
            var mapper = new BasicTypeMapper<int, int>();

            RunCollectionsTest<int, IEnumerable<Source<int>>, List<Destination<int>>>(
                mapper,
                () => ar.Select(p => new Source<int> { Value = p }),
                () => ar.Select(p => new Destination<int> { Value = p }).ToList()
                );

            RunCollectionsTest<int, IEnumerable<Source<int>>, IList<Destination<int>>>(
                mapper,
                () => ar.Select(p => new Source<int> { Value = p }),
                () => ar.Select(p => new Destination<int> { Value = p }).ToList()
                );
        }

        [Fact]
        public void IEnumerableToCollection()
        {
            var ar = new int[] { 1, 2, 3 };
            
            var mapper = new BasicTypeMapper<int, int>();

            RunCollectionsTest<int, IEnumerable<Source<int>>, Collection<Destination<int>>>(
                mapper,
                () => ar.Select(p => new Source<int> { Value = p }),
                () => new Collection<Destination<int>>(ar.Select(p => new Destination<int> { Value = p }).ToList())
                );


            RunCollectionsTest<int, IEnumerable<Source<int>>, ICollection<Destination<int>>>(
                mapper,
                () => ar.Select(p => new Source<int> { Value = p }),
                () => new Collection<Destination<int>>(ar.Select(p => new Destination<int> { Value = p }).ToList())
                );
        }
        
        [Fact]
        public void IEnumerableToHashSet()
        {
            var ar = new int[] { 1, 2, 3 };
            
            var mapper = new BasicTypeMapper<int, int>();

            RunCollectionsTest<int, IEnumerable<Source<int>>, HashSet<Destination<int>>>(
                mapper,
                () => ar.Select(p => new Source<int> { Value = p }),
                () => new HashSet<Destination<int>>(ar.Select(p => new Destination<int> { Value = p }))
                );
        }

        [Fact]
        public void IEnumerableToArray()
        {
            var ar = new int[] { 1, 2, 3 };
            
            var mapper = new BasicTypeMapper<int, int>();

            RunCollectionsTest<int, IEnumerable<Source<int>>, Destination<int>[]>(
                mapper,
                () => ar.Select(p => new Source<int> { Value = p }),
                () => ar.Select(p => new Destination<int> { Value = p }).ToArray()
                );
        }

        [Fact]
        public void CanDoExplicitCast()
        {
            var source = new Source<long> { Value = 100 };
            var expected = new Destination<int> { Value = 100 };
            var result = new BasicLongMapper().Map(source);

            Assert.Equal(expected, result);
        }

        private void RunCollectionsTest<TSource, TSourceCollection, TDestinationCollection>(
            object mapper, 
            Func<TSourceCollection> sourceValues,
            Func<TDestinationCollection> expectedValues)
            where TSourceCollection : IEnumerable<Source<TSource>>
            where TDestinationCollection : IEnumerable<Destination<TSource>>
        {
            var m = mapper as IMapper<TSourceCollection, TDestinationCollection>;

            Assert.NotNull(m);

            var result = m!.Map(sourceValues());

            Assert.Equal(expectedValues(), result);
        }

        public static IEnumerable<object[]> SimpleTypes =>
            new List<object[]>
            {
                new object[] { typeof(int), 10 },
                new object[] { typeof(int?), 10 },
                new object[] { typeof(string), "Text" },
                new object[] { typeof(double), 10.11 },
                new object[] { typeof(double?), 10.11 },
                new object[] { typeof(DateTime), DateTime.Now },
                new object[] { typeof(DateTime?), DateTime.Now },
            };
        public static IEnumerable<object[]> SimpleTypesCollection =>
            new List<object[]>
            {
                new object[] { typeof(int), 10 },
                new object[] { typeof(int?), 10 },
                new object[] { typeof(string), "Text" },
                new object[] { typeof(double), 10.11 },
                new object[] { typeof(double?), 10.11 },
                new object[] { typeof(DateTime), DateTime.Now },
                new object[] { typeof(DateTime?), DateTime.Now },
            };

        private void RunTest<TSource>(TSource sourceValue)
        {
            RunTest2(sourceValue, sourceValue);
        }

        private void RunTest2<TSource, TDestination>(TSource sourceValue, TDestination expectedValue)
        {
            var source = new Source<TSource> { Value = sourceValue };
            var expected = new Destination<TDestination> { Value = expectedValue };

            var mapper = new BasicTypeMapper<TSource, TDestination>();
            var result = mapper.Map(source);

            Assert.Equal<Destination<TDestination>>(expected, result);
        }
    }

    [MappingGenerator(typeof(Source<int>), typeof(Destination<int>))]
    public partial class BasicIntMapper
    {
    }

    [MappingGenerator(typeof(Source<long>), typeof(Destination<int>))]
    public partial class BasicLongMapper
    {
    }

    [MappingGenerator(typeof(Source<>), typeof(Destination<>))]
    public partial class BasicTypeMapper<TSource, TDestination>
    {
        private TDestination Convert(TSource source)
        {
            if (source is TDestination d)
                return d;

            throw new InvalidOperationException($"Can't convert {typeof(TSource)} to {typeof(TDestination)}");
        }
    }
}
