using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    public static class CollectionsHelper
    {
        public static List<TOut> CopyToNewList<TIn, TOut>(IEnumerable<TIn>? source, Converter<TIn, TOut> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (source == null)
                return new List<TOut>();

            return new List<TOut>(source.Select(p => mapper(p)));
        }

        public static HashSet<TOut> CopyToNewHashSet<TIn, TOut>(IEnumerable<TIn>? source, Converter<TIn, TOut> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (source == null)
                return new HashSet<TOut>();

            return new HashSet<TOut>(source.Select(p => mapper(p)));
        }

        public static Collection<TOut> CopyToNewCollection<TIn, TOut>(IEnumerable<TIn>? source, Converter<TIn, TOut> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            var list = CopyToNewList(source, mapper);

            return new Collection<TOut>(list);
        }
        
        public static TOut[] CopyToNewArray<TIn, TOut>(IEnumerable<TIn>? source, Converter<TIn, TOut> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (source == null)
                return Array.Empty<TOut>();

            return source.Select(p => mapper(p)).ToArray();
        }
        
        public static Dictionary<TOutKey, TOutValue> CopyToNewDictionary<TInKey, TInValue, TOutKey, TOutValue>(
            ICollection<KeyValuePair<TInKey, TInValue>>? source, 
            Converter<KeyValuePair<TInKey, TInValue>, KeyValuePair<TOutKey, TOutValue>> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (source == null)
                return new Dictionary<TOutKey, TOutValue>();

            var result = new Dictionary<TOutKey, TOutValue>(source.Count);

            foreach (var kv in source)
            {
                var s = mapper(kv);
                result.Add(s.Key, s.Value); 
            }

            return result;
        }

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

        public static List<T> CopyToNewList<T>(IEnumerable<T>? source)
        {
            if (source == null)
                return new List<T>();

            return new List<T>(source);
        }

        public static HashSet<T> CopyToNewHashSet<T>(IEnumerable<T>? source)
        {
            if (source == null)
                return new HashSet<T>();

            return new HashSet<T>(source);
        }

        public static Collection<T> CopyToNewCollection<T>(IEnumerable<T>? source)
        {
            if (source == null)
                return new Collection<T>();

            var list = CopyToNewList(source);

            return new Collection<T>(list);
        }

        public static T[] CopyToNewArray<T>(IEnumerable<T>? source)
        {
            if (source == null)
                return Array.Empty<T>();

            return source.ToArray();
        }

        public static Dictionary<TKey, TValue> CopyToNewDictionary<TKey, TValue>(
            ICollection<KeyValuePair<TKey, TValue>>? source)
        {
            if (source == null)
                return new Dictionary<TKey, TValue>();

            var result = new Dictionary<TKey, TValue>(source.Count);

            foreach (var kv in source)
                result.Add(kv.Key, kv.Value);

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
    }
}
