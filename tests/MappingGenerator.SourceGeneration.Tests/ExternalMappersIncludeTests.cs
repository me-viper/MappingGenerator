using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;

using Talk2Bits.MappingGenerator.SourceGeneration;

using Xunit;
using Xunit.Abstractions;

namespace MappingGenerator.SourceGeneration.Tests
{
    public class ExternalMappersIncludeTests : CompilationDependentTests
    {
#if DEBUG
        private const string BasePath = @"..\..\..\..\MappingGenerator.ExternalMappers\bin\Debug\net6.0";
#else
        private const string BasePath = @"..\..\..\..\MappingGenerator.ExternalMappers\bin\Release\net6.0";
#endif

        public ExternalMappersIncludeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GetByType()
        {
            var code = @"
using Talk2Bits.MappingGenerator.Abstractions;
using MappingGenerator.ExternalMappers;

[assembly: MappingGeneratorIncludeMapper(typeof(ExternalMapper)) ]

namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
";
            var compilation = CreateCompilation(
                code,
                new[] 
                { 
                    MetadataReference.CreateFromFile(@$"{BasePath}\MappingGenerator.ExternalMappers.dll"),
                    MetadataReference.CreateFromFile(@$"{BasePath}\Talk2Bits.MappingGenerator.dll"),
                });
            
            var diagnostics = compilation.GetDiagnostics();
            var errors = diagnostics.Where(p => p.Severity > DiagnosticSeverity.Warning);
            Assert.Empty(errors);

            var context = new GeneratorContext(compilation, p => { });
            var result = MappingSourceGenerator.GetExternalMappers(compilation, context);

            Assert.Single(result);
            Assert.Equal("ExternalMapper", result.Single().Mapper.Name);
        }

        [Fact]
        public void NotValidMapperType()
        {
            var code = @"
using Talk2Bits.MappingGenerator.Abstractions;
using MappingGenerator.ExternalMappers;

[assembly: MappingGeneratorIncludeMapper(typeof(ExternalSource)) ]

namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
";
            var compilation = CreateCompilation(
                code,
                new[]
                {
                    MetadataReference.CreateFromFile(@$"{BasePath}\MappingGenerator.ExternalMappers.dll"),
                    MetadataReference.CreateFromFile(@$"{BasePath}\Talk2Bits.MappingGenerator.dll"),
                });

            var diagnostics = compilation.GetDiagnostics();
            var errors = diagnostics.Where(p => p.Severity > DiagnosticSeverity.Warning);
            Assert.Empty(errors);
            
            var context = new GeneratorContext(compilation, p => { });
            Assert.Throws<MappingGenerationException>(() => MappingSourceGenerator.GetExternalMappers(compilation, context));
        }

        [Fact]
        public void GetByAssembly()
        {
            var code = @"
using Talk2Bits.MappingGenerator.Abstractions;
using MappingGenerator.ExternalMappers;

[assembly: MappingGeneratorIncludeAssemblyAttribute(""MappingGenerator.ExternalMappers"") ]

namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
";
            var compilation = CreateCompilation(
                code,
                new[]
                {
                    MetadataReference.CreateFromFile(@$"{BasePath}\MappingGenerator.ExternalMappers.dll"),
                    MetadataReference.CreateFromFile(@$"{BasePath}\Talk2Bits.MappingGenerator.dll"),
                });

            var diagnostics = compilation.GetDiagnostics();
            var errors = diagnostics.Where(p => p.Severity > DiagnosticSeverity.Warning);
            Assert.Empty(errors);

            var context = new GeneratorContext(compilation, p => { });
            var result = MappingSourceGenerator.GetExternalMappers(compilation, context);

            Assert.Single(result);
            Assert.Equal("ExternalMapper", result.Single().Mapper.Name);
        }

        [Fact]
        public void NotValidAssembly()
        {
            var code = @"
using Talk2Bits.MappingGenerator.Abstractions;
using MappingGenerator.ExternalMappers;

[assembly: MappingGeneratorIncludeAssemblyAttribute(""MappingGenerator.Bad"") ]

namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
";
            var compilation = CreateCompilation(
                code,
                new[]
                {
                    MetadataReference.CreateFromFile(@$"{BasePath}\MappingGenerator.ExternalMappers.dll"),
                    MetadataReference.CreateFromFile(@$"{BasePath}\Talk2Bits.MappingGenerator.dll"),
                });

            var diagnostics = compilation.GetDiagnostics();
            var errors = diagnostics.Where(p => p.Severity > DiagnosticSeverity.Warning);
            Assert.Empty(errors);

            var context = new GeneratorContext(compilation, p => { });
            Assert.Throws<MappingGenerationException>(() => MappingSourceGenerator.GetExternalMappers(compilation, context));
        }
    }
}
