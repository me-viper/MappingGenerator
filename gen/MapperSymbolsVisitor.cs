using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class MapperSymbolsVisitor : SymbolVisitor
    {
        private readonly HashSet<INamedTypeSymbol> _symbols;
        private readonly INamedTypeSymbol _type;

        public MapperSymbolsVisitor(INamedTypeSymbol type, HashSet<INamedTypeSymbol> symbols)
        {
            _symbols = symbols;
            _type = type;
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var namespaceOrTypeSymbol in symbol.GetMembers())
                namespaceOrTypeSymbol.Accept(this);
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (symbol.Interfaces.Any(p => SymbolEqualityComparer.Default.Equals(_type, p.OriginalDefinition)))
                _symbols.Add(symbol);
        }
    }
}
