
using System;

using Microsoft.CodeAnalysis;

namespace MappingGenerator.SourceGeneration
{
    public interface IMappingSourceGeneratorContext
    {
        Compilation Compilation { get; }

        Action<Diagnostic> ReportDiagnostic { get; }
    }
}
