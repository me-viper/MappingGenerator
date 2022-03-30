namespace Talk2Bits.MappingGenerator.Abstractions
{
    public enum ConstructorAccessibility
    {
        /// <summary>
        /// Constructor will be public.
        /// </summary>
        Public = 0,
        
        /// <summary>
        /// Constructor will be private.
        /// </summary>
        Private = 1,
        
        /// <summary>
        /// Constructor will be private protected.
        /// </summary>
        PrivateProtected = 2,
        
        /// <summary>
        /// Constructor will be protected.
        /// </summary>        
        Protected = 3,
        
        /// <summary>
        /// Constructor will be internal.
        /// </summary>
        Internal = 4,
        
        /// <summary>
        /// Constructor will be internal protected.
        /// </summary>
        InternalProtected = 5,
        
        /// <summary>
        /// Suppress constructor generation.
        /// </summary>
        Suppress = 6
    }
}
