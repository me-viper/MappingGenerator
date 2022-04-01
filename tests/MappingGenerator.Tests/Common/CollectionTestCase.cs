using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Talk2Bits.MappingGenerator.Abstractions;

using Xunit;

namespace MappingGenerator.Tests.Common
{
    public interface ICollectionTestCase
    {
        void TestMapping();
    }

    public class CollectionTestCase<TIn, TDestination, TOut> : ICollectionTestCase
        where TDestination : IDestination<TOut>
    {
        private Func<TIn> _produceSource;

        private TOut? _produceExpected;

        private IMapper<Source<TIn>, TDestination> _mapper;

        public CollectionTestCase(
            Func<TIn> sourceValue,
            TOut? expected,
            IMapper<Source<TIn>, TDestination> mapper)
        {
            _produceSource = sourceValue;
            _produceExpected = expected;

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public void TestMapping()
        {
            var source = new Source<TIn> { Value = _produceSource() };
            var expected = _produceExpected;
            var result = _mapper.Map(source);

            if (expected == null)
            {
                Assert.Null(result.GetValue());
                return;
            }

            Assert.False(ReferenceEquals(expected, result.GetValue()));
            Assert.Equal(expected, result.GetValue());
        }

        public override string? ToString()
        {
            return $"<{typeof(TDestination)}> {typeof(TIn)} => {typeof(TOut)}";
        }
    }
}
