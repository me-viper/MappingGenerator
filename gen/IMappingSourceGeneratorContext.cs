using System;

using Microsoft.CodeAnalysis;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    public interface IMappingSourceGeneratorContext
    {
        Compilation Compilation { get; }

        Action<Diagnostic> ReportDiagnostic { get; }
    }
}
