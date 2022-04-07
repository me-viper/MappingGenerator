using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    public static class MapperExtensions
    {
        public static IEnumerable<TOut> Map<TIn, TOut>(this IMapper<TIn, TOut> mapper, IEnumerable<TIn>? source)
        {
            return ToList(mapper, source);
        }

        public static List<TOut> ToList<TIn, TOut>(this IMapper<TIn, TOut> mapper, IEnumerable<TIn>? source)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return CollectionsHelper.CopyToNewList(source, p => mapper.Map(p));
        }

        public static Collection<TOut> ToCollection<TIn, TOut>(this IMapper<TIn, TOut> mapper, IEnumerable<TIn>? source)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return CollectionsHelper.CopyToNewCollection(source, p => mapper.Map(p));
        }

        public static HashSet<TOut> ToHashSet<TIn, TOut>(this IMapper<TIn, TOut> mapper, IEnumerable<TIn>? source)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return CollectionsHelper.CopyToNewHashSet(source, p => mapper.Map(p));
        }

        public static TOut[] ToArray<TIn, TOut>(this IMapper<TIn, TOut> mapper, IEnumerable<TIn>? source)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return CollectionsHelper.CopyToNewArray(source, p => mapper.Map(p));
        }
    }
}
