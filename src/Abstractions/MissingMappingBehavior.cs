namespace Talk2Bits.MappingGenerator.Abstractions
{
    public enum MissingMappingBehavior
    {
        /// <summary>
        /// Missing mapping will cause compiler warning.
        /// </summary>
        Warning,
        
        /// <summary>
        /// Missing mapping will be silently ignored.
        /// </summary>
        Ignore,
        
        /// <summary>
        /// Missing mapping will cause compiler error.
        /// </summary>
        Error
    }
}
