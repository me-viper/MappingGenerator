namespace Talk2Bits.MappingGenerator.Abstractions
{
    public interface IMapper<in TSource, out TDestination>
    {
        TDestination Map(TSource source);
    }
}