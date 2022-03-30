using System;

using Microsoft.CodeAnalysis;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    internal class GeneratorContext : IGeneratorContext
    {
        public Compilation Compilation { get; }

        public Action<Diagnostic> ReportDiagnostic { get; }

        public GeneratorContext(Compilation compilation, Action<Diagnostic> reportDiagnostic)
        {
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            ReportDiagnostic = reportDiagnostic ?? throw new ArgumentNullException(nameof(reportDiagnostic));
        }
    }
}
