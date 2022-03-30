using Microsoft.CodeAnalysis;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal record MappingDestination(
        string Name,
        ITypeSymbol Type,
        ISymbol DeclaringSymbol,
        CustomizedMapping? Options,
        MappingDestinationType EntryType) : MappingDefinition(Name, Type, DeclaringSymbol, Options)
    {
        public static MappingDestination FromDefinition(MappingDefinition definition, MappingDestinationType destinationType)
        {
            return new MappingDestination(
                definition.Name,
                definition.Type,
                definition.DeclaringSymbol, 
                definition.Options,
                destinationType
                );
        }

        public string SourceName { get; } = Options?.Source.Name ?? Name;

        public bool IsReadable()
        {
            if (DeclaringSymbol is IPropertySymbol prop)
            {
                if (prop.IsWriteOnly)
                    return false;

                if (prop.GetMethod == null)
                    return false;
            }
            
            return true;
        }
        
        public bool IsWritable()
        {
            if (EntryType == MappingDestinationType.Parameter)
                return true;

            if (DeclaringSymbol is IPropertySymbol prop)
            {
                if (prop.IsReadOnly)
                    return false;

                if (prop.SetMethod == null)
                    return false;

                if (EntryType == MappingDestinationType.Property && prop.SetMethod.IsInitOnly)
                    return false;

                return true; 
            }

            return false;
        }

        public MappingDefinitionEqualityComparer Comparer { get; } =
            EntryType == MappingDestinationType.Parameter
                ? MappingDefinitionEqualityComparer.IgnoreCase
                : MappingDefinitionEqualityComparer.Default;
    }
}
