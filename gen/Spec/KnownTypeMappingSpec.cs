namespace Talk2Bits.MappingGenerator.SourceGeneration.Spec
{
    internal record KnownTypeMappingSpec(
        string MemberName,
        KnownMapper Mapper,
        MappingDestination Destination,
        bool IsInternal) : MappingSpec(Destination);
}