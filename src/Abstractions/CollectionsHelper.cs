using System;
using System.Collections.Generic;
using System.Linq;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    public static class CollectionsHelper
    {
        public static TCollection CopyToNew<TIn, TOut, TCollection>(IEnumerable<TIn>? source, Converter<TIn, TOut> mapper)
            where TCollection : ICollection<TOut>, new()
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            var result = new TCollection();

            if (source == null)
                return result;

            foreach (var src in source)
                result.Add(mapper(src));

            return result;
        }

        public static TCollection CopyToNew<TIn, TOut, TCollection>(IEnumerable<TIn>? source, IMapper<TIn, TOut> mapper)
            where TCollection : ICollection<TOut>, new()
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            var result = new TCollection();

            if (source == null)
                return result;

            foreach (var src in source)
                result.Add(mapper.Map(src));

            return result;
        }

        public static TCollection CopyToNew<T, TCollection>(IEnumerable<T>? source)
            where TCollection : ICollection<T>, new()
        {
            var result = new TCollection();

            if (source == null)
                return result;

            foreach (var src in source)
                result.Add(src);

            return result;
        }

        public static void CopyTo<T>(
            IEnumerable<T>? source,
            ICollection<T>? destination)
        {
            if (destination == null)
                return;

            if (ReferenceEquals(source, destination))
                return;

            destination.Clear();

            source ??= Enumerable.Empty<T>();

            foreach (var src in source)
                destination.Add(src);
        }

        public static void CopyTo<TIn, TOut>(
            IEnumerable<TIn>? source,
            ICollection<TOut>? destination,
            Converter<TIn, TOut> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (destination == null)
                return;

            if (ReferenceEquals(source, destination))
                return;

            destination.Clear();

            source ??= Enumerable.Empty<TIn>();

            foreach (var src in source)
                destination.Add(mapper(src));
        }

        public static void CopyTo<TIn, TOut>(
            IEnumerable<TIn>? source, 
            ICollection<TOut>? destination,
            IMapper<TIn, TOut> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (destination == null)
                return;

            if (ReferenceEquals(source, destination))
                return;

            destination.Clear();

            source ??= Enumerable.Empty<TIn>();

            foreach (var src in source)
                destination.Add(mapper.Map(src));
        }
    }
}
