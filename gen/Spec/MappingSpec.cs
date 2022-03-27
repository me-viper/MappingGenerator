using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Talk2Bits.MappingGenerator.SourceGeneration.Spec
{
    internal record MappingSpec(MappingDestination Destination)
    {
        public List<ExpressionSyntax> MappingExpressions { get; } = new();

        public List<StatementSyntax> MappingStatements { get; } = new();

        public void TransformMappingExpressions(Func<ExpressionSyntax, ExpressionSyntax> transformer)
        {
            var current = MappingExpressions.ToArray();
            MappingExpressions.Clear();
            MappingExpressions.AddRange(current.Select(transformer));
        }
    }
}