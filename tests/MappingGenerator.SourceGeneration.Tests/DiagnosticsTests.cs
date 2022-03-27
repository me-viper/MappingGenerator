using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading.Tasks;

using Talk2Bits.MappingGenerator.Abstractions;
using Talk2Bits.MappingGenerator.SourceGeneration;

using Xunit;

namespace MappingGenerator.SourceGeneration.Tests
{
    public class DiagnosticsTests
    {
        [Fact]
        public async Task NestedClass()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A
    {
        [MappingGeneratorAttribute(typeof(object), typeof(string))]
        public partial class TestMapper {}
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();

            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var expectedResult = DiagnosticResult
                .CompilerError("MG0007")
                .WithArguments("Test.A.TestMapper")
                .WithSpan(9, 9, 10, 43);

            generator.TestState.ExpectedDiagnostics.Add(expectedResult);

            await generator.RunAsync();
        }

        [Fact]
        public async Task PartialModifierMissing()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    [MappingGenerator(typeof(object), typeof(string))]
    public class TestMapper {}
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var expectedResult = DiagnosticResult
                .CompilerError("MG0002")
                .WithArguments("Test.TestMapper")
                .WithSpan(7, 5, 8, 31);

            generator.TestState.ExpectedDiagnostics.Add(expectedResult);

            await generator.RunAsync();
        }

//        [Fact]
//        public async Task NoCastExists()
//        {
//            var code = @"
//namespace Test
//{
//    using System;
//    using Talk2Bits.MappingGenerator.Abstractions;

//    public class A { public string Val { get; set; } };
//    public class B { public int Val { get; set;} }

//    [MappingGenerator(typeof(A), typeof(B))]
//    public partial class TestMapper {}
//}
//";
//            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
//            generator.TestState.Sources.Add(code);
//            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

//            var d1 = DiagnosticResult
//                .CompilerError("MG0001")
//                .WithArguments("Test.TestMapper", "source.Val", "string", "int")
//                .WithNoLocation();

//            generator.TestState.ExpectedDiagnostics.Add(d1);

//            await generator.RunAsync();
//        }

        [Fact]
        public async Task InvalidMappingMethodParameterCount()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A { public string Val { get; set;} }
    public class B { public string Val { get; set;} }

    [MappingGenerator(typeof(A), typeof(B))]
    public partial class TestMapper 
    {
        public string MapVal(A a, int b) 
        { 
            return string.Empty; 
        }
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerWarning("MG0003")
                .WithArguments("Test.TestMapper", "Test.A")
                .WithSpan(13, 23, 13, 29);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        private static string MappingMissingSource(MissingMappingBehavior behavior) => $@"
namespace Test
{{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {{ }}
    public class B {{ public string Val {{ get; set; }} }}

    [MappingGenerator(typeof(A), typeof(B), MissingMappingBehavior = MissingMappingBehavior.{behavior.ToString("f")})]
    public partial class TestMapper 
    {{
    }}
}}
";

        [Fact]
        public async Task MappingMissingError()
        {
            var code = MappingMissingSource(MissingMappingBehavior.Error);

            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0004")
                .WithArguments("Test.TestMapper", "Test.B", "Val")
                .WithNoLocation();

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        [Fact]
        public async Task MappingMissingWarning()
        {
            var code = MappingMissingSource(MissingMappingBehavior.Warning);

            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerWarning("MG0004")
                .WithArguments("Test.TestMapper", "Test.B", "Val")
                .WithNoLocation();

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        [Fact]
        public async Task MappingMissingIgnore()
        {
            var code = MappingMissingSource(MissingMappingBehavior.Ignore);

            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            await generator.RunAsync();
        }

        [Fact]
        public async Task InvalidConfiguration()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {}
    public class B {}

    [MappingGenerator(typeof(A), typeof(B))]
    [MappingGeneratorPropertyMapping(""AP"", ""BP"")]
    public partial class TestMapper {}
}
";

            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0005")
                .WithArguments("Test.TestMapper", "Source", "Test.A", "AP")
                .WithSpan(12, 26, 12, 36);

            var d2 = DiagnosticResult
                .CompilerError("MG0005")
                .WithArguments("Test.TestMapper", "Destination", "Test.B", "BP")
                .WithSpan(12, 26, 12, 36);

            generator.TestState.ExpectedDiagnostics.Add(d2);
            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        [Fact]
        public async Task BadConstructorMethodSignatureArgs()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {}
    public class B {}

    [MappingGenerator(typeof(A), typeof(B))]
    public partial class TestMapper 
    {
        private B CreateDestination() { return null; }

        private B CreateDestination(string a) { return null; }
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerWarning("MG0006")
                .WithArguments("Test.TestMapper", "Test.A", "Test.B")
                .WithSpan(13, 19, 13, 36);

            var d2 = DiagnosticResult
                .CompilerWarning("MG0006")
                .WithArguments("Test.TestMapper", "Test.A", "Test.B")
                .WithSpan(15, 19, 15, 36);

            generator.TestState.ExpectedDiagnostics.Add(d1);
            generator.TestState.ExpectedDiagnostics.Add(d2);

            await generator.RunAsync();
        }

        [Fact]
        public async Task BadConstructorMethodSignatureReturnType()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {}
    public class B {}

    [MappingGenerator(typeof(A), typeof(B))]
    public partial class TestMapper 
    {
        private A CreateDestination(A source) { return null; }
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0009")
                .WithArguments("Test.TestMapper", "Test.B", "Test.A")
                .WithSpan(13, 19, 13, 36);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        [Fact]
        public async Task InvalidTypeParametersNumber()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A<T> {}
    public class B<T> {}

    [MappingGenerator(typeof(A<>), typeof(B<>))]
    public partial class TestMapper<T>
    {
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0010")
                .WithArguments("Test.TestMapper<T>", 1, 2)
                .WithSpan(11, 26, 11, 36);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        [Fact]
        public async Task NoMatchingConstructor()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {}
    public class B 
    {
        public B(string s) {}
    }

    [MappingGenerator(typeof(A), typeof(B))]
    public partial class TestMapper 
    {
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0008")
                .WithArguments("Test.TestMapper", "Test.B", "Test.A")
                .WithSpan(8, 18, 8, 19);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }
    
        [Fact]
        public async Task SourcePropertyMissing()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A { public string AA { get; set; } }
    public class B { public string BB { get; set; } }

    [MappingGenerator(typeof(A), typeof(B))]
    [MappingGeneratorPropertyMapping(""CC"", ""BB"")]
    public partial class TestMapper 
    {
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0005")
                .WithArguments("Test.TestMapper", "Source", "Test.A", "CC")
                .WithSpan(12, 26, 12, 36);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }
        
        [Fact]
        public async Task DestinationPropertyMissing()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A { public string AA { get; set; } }
    public class B { public string BB { get; set; } }

    [MappingGenerator(typeof(A), typeof(B))]
    [MappingGeneratorPropertyMapping(""AA"", ""CC"")]
    public partial class TestMapper 
    {
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0005")
                .WithArguments("Test.TestMapper", "Destination", "Test.B", "CC")
                .WithSpan(12, 26, 12, 36);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        [Fact]
        public async Task NoDestinationForMappingMethod()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {}
    public class B {}

    [MappingGenerator(typeof(A), typeof(B))]
    public partial class TestMapper 
    {
        private string MapX(A source)
        {
            return null;
        }
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerWarning("MG0011")
                .WithArguments("Test.TestMapper", "Test.TestMapper.MapX(Test.A)", "Test.B", "X")
                .WithSpan(13, 24, 13, 28);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        [Fact]
        public async Task NameDuplication()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {}
    public class B {}

    [MappingGenerator(typeof(A), typeof(B), Name = ""A"")]
    [MappingGenerator(typeof(B), typeof(A), Name = ""A"")]
    public partial class TestMapper 
    {
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0012")
                .WithArguments("Test.TestMapper", "A")
                .WithSpan(12, 26, 12, 36);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        [Theory]
        [InlineData("$asd")]
        [InlineData("#asd")]
        [InlineData("A B")]
        [InlineData(" AB")]
        [InlineData("AB ")]
        public async Task InvalidName(string mapperName)
        {
            string Code(string name) => @$"
namespace Test
{{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {{}}
    public class B {{}}

    [MappingGenerator(typeof(A), typeof(B), Name = ""{name}"")]
    public partial class TestMapper 
    {{
    }}
}}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(Code(mapperName));
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0013")
                .WithArguments("Test.TestMapper", mapperName)
                .WithSpan(11, 26, 11, 36);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        [Fact]
        public async Task InconsistentConstructorAccessibility()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {}
    public class B {}

    [MappingGenerator(typeof(A), typeof(B), ConstructorAccessibility = ConstructorAccessibility.Private)]
    [MappingGenerator(typeof(B), typeof(A), ConstructorAccessibility = ConstructorAccessibility.Public)]
    public partial class TestMapper 
    {
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0014")
                .WithArguments("Test.TestMapper")
                .WithSpan(12, 26, 12, 36);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        [Fact]
        public async Task MapperConflictNamed()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {}
    public class B {}
    public class C {}

    [MappingGenerator(typeof(A), typeof(B), Name = ""M"")]
    [MappingGenerator(typeof(A), typeof(C))]
    public partial class TestMapper 
    {
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0015")
                .WithArguments("Test.TestMapper", "A to B (Name = M)", "Test.A")
                .WithSpan(13, 26, 13, 36);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        [Fact]
        public async Task MapperConflict()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {}
    public class B {}
    public class C {}

    [MappingGenerator(typeof(A), typeof(B))]
    [MappingGenerator(typeof(A), typeof(C))]
    public partial class TestMapper 
    {
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0015")
                .WithArguments("Test.TestMapper", "A to B", "Test.A")
                .WithSpan(13, 26, 13, 36);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }

        //        [Fact]
        //        public async Task MultipleGenericsNotSupported()
        //        {
        //            var code = @"
        //namespace Test
        //{
        //    using System;
        //    using Talk2Bits.MappingGenerator.Abstractions;

        //    public class A<T> {}
        //    public class B<T> {}
        //    public class C<T> {}

        //    [MappingGenerator(typeof(A<>), typeof(B<>))]
        //    [MappingGenerator(typeof(C<>), typeof(A<>))]
        //    public partial class TestMapper<T1, T2> 
        //    {
        //    }
        //}
        //";
        //            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
        //            generator.TestState.Sources.Add(code);
        //            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

        //            var d1 = DiagnosticResult
        //                .CompilerError("MG0016")
        //                .WithArguments("Test.TestMapper")
        //                .WithSpan(13, 26, 13, 36);

        //            generator.TestState.ExpectedDiagnostics.Add(d1);

        //            await generator.RunAsync();
        //        }

        [Fact]
        public async Task MultipleGenericsNotSupported()
        {
            var code = @"
namespace Test
{
    using System;
    using Talk2Bits.MappingGenerator.Abstractions;

    public class A {}
    public class B {}

    [MappingGenerator(typeof(A), typeof(B))]
    [MappingGenerator(typeof(A), typeof(B), Name = ""M"")]
    public partial class TestMapper
    {
    }
}
";
            var generator = new CSharpSourceGeneratorVerifier<MappingSourceGenerator>.Test();
            generator.TestState.Sources.Add(code);
            generator.TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;

            var d1 = DiagnosticResult
                .CompilerError("MG0017")
                .WithArguments("Test.TestMapper", "A to B", "Test.A", "Test.B")
                .WithSpan(12, 26, 12, 36);

            generator.TestState.ExpectedDiagnostics.Add(d1);

            await generator.RunAsync();
        }
    }
}