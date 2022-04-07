using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.DependencyInjection
{
    public class AggregatedMapper : IMapper
    {
        private readonly ConcurrentDictionary<MapperInfo, Delegate> _mappers = new(MapperInfoEqualityComparer.Instance);

        public AggregatedMapper(IEnumerable<IAbstractMapper> mappers)
        {
            if (mappers == null)
                throw new ArgumentNullException(nameof(mappers));

            foreach (var mapper in mappers)
            {
                foreach (var impl in mapper.GetType().GetInterfaces())
                {
                    if (!impl.IsGenericType || impl.GetGenericTypeDefinition() != typeof(IMapper<,>))
                        continue;

                    var mapperInfo = new MapperInfo(impl.GenericTypeArguments[0], impl.GenericTypeArguments[1]);

                    if (_mappers.ContainsKey(mapperInfo))
                        continue;

                    var mapMethod = impl.GetMethod("Map");
                    var type = Expression.GetDelegateType(mapperInfo.Source, mapperInfo.Destination);
                    var mapDelegate = Delegate.CreateDelegate(type, mapper, mapMethod);
                    _mappers.TryAdd(mapperInfo, mapDelegate);

                    // Cache enumerables.
                    var enumerableSource = typeof(IEnumerable<>).MakeGenericType(mapperInfo.Source);
                    
                    var enumerableMapper = new MapperInfo(
                        enumerableSource,
                        typeof(IEnumerable<>).MakeGenericType(mapperInfo.Destination)
                        );
                    Register(enumerableMapper, mapperInfo, mapper, typeof(MapperExtensions).GetMethod(nameof(MapperExtensions.Map)));
                    
                    var listMapper = new MapperInfo(
                        enumerableSource,
                        typeof(List<>).MakeGenericType(mapperInfo.Destination)
                        );
                    Register(listMapper, mapperInfo, mapper, typeof(MapperExtensions).GetMethod(nameof(MapperExtensions.ToList)));
                    
                    var collectionMapper = new MapperInfo(
                        enumerableSource,
                        typeof(Collection<>).MakeGenericType(mapperInfo.Destination)
                        );
                    Register(collectionMapper, mapperInfo, mapper, typeof(MapperExtensions).GetMethod(nameof(MapperExtensions.ToCollection)));
                    
                    var hashSetMapper = new MapperInfo(
                        enumerableSource,
                        typeof(HashSet<>).MakeGenericType(mapperInfo.Destination)
                        );
                    Register(hashSetMapper, mapperInfo, mapper, typeof(MapperExtensions).GetMethod(nameof(MapperExtensions.ToHashSet)));
                    
                    var arrayMapper = new MapperInfo(
                        enumerableSource,
                        mapperInfo.Destination.MakeArrayType()
                        );
                    Register(arrayMapper, mapperInfo, mapper, typeof(MapperExtensions).GetMethod(nameof(MapperExtensions.ToArray)));
                }
            }
        }

        private void Register(MapperInfo registration, MapperInfo mapperImpl, object mapper, MethodInfo mapMethod)
        {
            var mi = mapMethod.MakeGenericMethod(mapperImpl.Source, mapperImpl.Destination);
            var type = Expression.GetDelegateType(registration.Source, registration.Destination);
            var mapDelegate = Delegate.CreateDelegate(type, mapper, mi);
            _mappers.TryAdd(registration, mapDelegate);
        }

        public TOut Map<TIn, TOut>(TIn source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var inType = typeof(TIn);
            var outType = typeof(TOut);

            var mi = new MapperInfo(inType, outType);

            if (_mappers.TryGetValue(mi, out var mapper))
            {
                var func = (Func<TIn, TOut>)mapper;
                return func(source);
            }

            foreach (var compatibleMapper in _mappers.Keys)
            {
                if (compatibleMapper.CanMap(mi))
                {
                    var func = (Func<TIn, TOut>)_mappers[compatibleMapper];
                    _mappers.TryAdd(mi, func);
                    return func(source);
                } 
            }

            throw new UnknownMappingException(inType, outType);
        }
    }
}
