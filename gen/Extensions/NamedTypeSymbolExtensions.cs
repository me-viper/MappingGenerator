using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Text;

namespace Talk2Bits.MappingGenerator.SourceGeneration.Extensions
{
    internal static class NamedTypeSymbolExtensions
    {
        public static string GetFriendlyName(this INamedTypeSymbol typeSymbol)
        {
            var parts = typeSymbol.ToDisplayParts();
            var result = new StringBuilder();

            foreach (var part in parts)
            {
                if (string.Equals(part.ToString(), ">", StringComparison.Ordinal))
                    continue;

                if (string.Equals(part.ToString(), ".", StringComparison.Ordinal))
                    continue;

                if (string.Equals(part.ToString(), "<", StringComparison.Ordinal))
                {
                    result.Append("Of");
                    continue;
                }

                if (string.Equals(part.ToString(), ",", StringComparison.Ordinal))
                {
                    result.Append("And");
                    continue;
                }

                result.Append(part.ToString());
            }

            return result.ToString();
        }
    }
}
