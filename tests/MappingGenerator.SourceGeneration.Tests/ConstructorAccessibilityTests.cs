using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Talk2Bits.MappingGenerator.Abstractions;
using Talk2Bits.MappingGenerator.SourceGeneration;

using Xunit;
using Xunit.Abstractions;

namespace MappingGenerator.SourceGeneration.Tests
{
    internal class SyntaxTokenEqualityComparer : IEqualityComparer<SyntaxToken>
    {
        public bool Equals(SyntaxToken x, SyntaxToken y)
        {
            return string.Equals(x.Text, y.Text, StringComparison.Ordinal);
        }

        public int GetHashCode([DisallowNull] SyntaxToken obj)
        {
            return obj.GetHashCode();
        }
    }

    public class ConstructorAccessibilityTests : CompilationDependentTests
    {
        public ConstructorAccessibilityTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        //[InlineData(ConstructorAccessibility.Public)]
        [InlineData(ConstructorAccessibility.Private)]
        [InlineData(ConstructorAccessibility.PrivateProtected)]
        [InlineData(ConstructorAccessibility.InternalProtected)]
        [InlineData(ConstructorAccessibility.Protected)]
        public void Modifiers(ConstructorAccessibility accessibility)
        {
            static string Code(ConstructorAccessibility access) => $@"
namespace Test
{{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {{}}
    public class B {{}}

    [MappingGeneratorAttribute(typeof(A), typeof(B), ConstructorAccessibility = ConstructorAccessibility.{access.ToString("f")} )]
    public partial class TestMapper {{}}
}}
";
            void ValidAccessor(SyntaxTree syntaxTree)
            {
                Assert.NotNull(syntaxTree);

                Output.WriteLine(syntaxTree.ToString());

                var classSyntax = syntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(p => string.Equals(p.Identifier.ToString(), "TestMapper", StringComparison.Ordinal))
                    .SingleOrDefault();

                Assert.NotNull(classSyntax);

                var ctors = classSyntax!
                    .DescendantNodes()
                    .OfType<ConstructorDeclarationSyntax>()
                    .ToList();

                Assert.Single(ctors);

                var expectedModifiers = SyntaxFactory.TokenList(MappingSyntaxFactory.GetAccessibilityModifier(accessibility));

                Assert.Equal(
                    expectedModifiers.ToArray(), 
                    ctors[0].Modifiers.ToArray(), 
                    new SyntaxTokenEqualityComparer()
                    );

            }

            var generator = new MappingSourceGenerator();

            var compilation = CreateCompilation(Code(accessibility));
            var diagnostics = compilation.GetDiagnostics();
            var errors = diagnostics.Where(p => p.Severity > DiagnosticSeverity.Warning);
            Assert.Empty(errors);

            var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

            var result = driver.GetRunResult();
            Assert.Collection(result.GeneratedTrees, ValidAccessor);
        }
    }
}
