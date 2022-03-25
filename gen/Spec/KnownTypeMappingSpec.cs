namespace MappingGenerator.SourceGeneration.Spec
{
    internal record KnownTypeMappingSpec(
        KnownMapper Mapper,
        MappingDestination Destination) : MappingSpec(Destination);
}