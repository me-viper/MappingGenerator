namespace Talk2Bits.MappingGenerator.DependencyInjection
{
    public interface IMapper
    {
        TOut Map<TIn, TOut>(TIn source);
    }
}
