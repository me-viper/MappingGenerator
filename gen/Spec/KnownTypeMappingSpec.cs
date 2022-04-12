namespace Talk2Bits.MappingGenerator.SourceGeneration.Spec
{
    internal record KnownTypeMappingSpec(
        string MemberName,
        KnownMapperRef Mapper,
        MappingDestination Destination,
        bool IsInternal) : MappingSpec(Destination);
}