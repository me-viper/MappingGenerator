
using System;

using Microsoft.CodeAnalysis;

namespace MappingGenerator.SourceGeneration
{
    internal class MappingSourceGeneratorContext : IMappingSourceGeneratorContext
    {
        public Compilation Compilation { get; }

        public Action<Diagnostic> ReportDiagnostic { get; }

        public MappingSourceGeneratorContext(Compilation compilation, Action<Diagnostic> reportDiagnostic)
        {
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            ReportDiagnostic = reportDiagnostic ?? throw new ArgumentNullException(nameof(reportDiagnostic));
        }
    }
}
