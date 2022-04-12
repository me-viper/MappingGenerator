using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Talk2Bits.MappingGenerator.Abstractions;
using Talk2Bits.MappingGenerator.SourceGeneration;

using Xunit.Abstractions;

namespace MappingGenerator.SourceGeneration.Tests
{
    public class CompilationDependentTests
    {
        protected ITestOutputHelper Output { get; }

        public CompilationDependentTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private MetadataReference MapperRef =
            MetadataReference.CreateFromFile(typeof(MappingGeneratorAttribute).GetTypeInfo().Assembly.Location);

        private IEnumerable<MetadataReference> DefaultReferences = new[]
        {
            MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location),
            MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
        };

        protected Compilation EmptyCompilation => CreateCompilation(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
");
        protected Compilation CreateCompilation(string source, IEnumerable<MetadataReference>? refs = null)
            => CSharpCompilation.Create(
                "compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                DefaultReferences.Concat(refs ?? new[] { MapperRef }),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                );
    }
}
