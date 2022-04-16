using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Talk2Bits.MappingGenerator.SourceGeneration;

using Xunit;

namespace MappingGenerator.SourceGeneration.Tests
{
    public class CollectionClassifierTests
    {
        private Compilation _compilation = CreateCompilation(@"
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
        private KnownTypeSymbols _knownTypes { get; }

        public CollectionClassifierTests()
        {
            _knownTypes = new KnownTypeSymbols(_compilation);
        }

        [Fact]
        public void IEnumerable()
        {
            var intType = _compilation.GetSpecialType(SpecialType.System_Int32);

            var classifier = new CollectionClassifier(_knownTypes);
            var sourceType = _knownTypes.IEnumerableType.Construct(intType);
            var entry = new MappingDefinition("Test", sourceType, null!, null);

            var result = classifier.ClassifyCollectionType(entry.Type);

            Assert.True(result.IsEnumerable);
            Assert.True(result.IsInterface);
            Assert.Equal(CollectionKind.List, result.CollectionKind);
            Assert.Equal(intType, result.ElementsType, SymbolEqualityComparer.Default);
        }

        [Fact]
        public void Array()
        {
            var intType = _compilation.GetSpecialType(SpecialType.System_Int32);

            var classifier = new CollectionClassifier(_knownTypes);
            var sourceType = _compilation.CreateArrayTypeSymbol(intType);
            var entry = new MappingDefinition("Test", sourceType, null!, null);

            var result = classifier.ClassifyCollectionType(entry.Type);

            Assert.True(result.IsEnumerable);
            Assert.True(result.IsArray);
            Assert.Equal(CollectionKind.Array, result.CollectionKind);
            Assert.Equal(intType, result.ElementsType, SymbolEqualityComparer.Default);
        }

        [Fact]
        public void List()
        {
            var intType = _compilation.GetSpecialType(SpecialType.System_Int32);

            var classifier = new CollectionClassifier(_knownTypes);
            var sourceType = _knownTypes.ListType.Construct(intType);
            var entry = new MappingDefinition("Test", sourceType, null!, null);

            var result = classifier.ClassifyCollectionType(entry.Type);

            Assert.True(result.IsEnumerable);
            Assert.True(result.IsType);
            Assert.True(result.IsCollection);
            Assert.Equal(CollectionKind.List, result.CollectionKind);
            Assert.Equal(intType, result.ElementsType, SymbolEqualityComparer.Default);
        }

        [Fact]
        public void ICollection()
        {
            var intType = _compilation.GetSpecialType(SpecialType.System_Int32);

            var classifier = new CollectionClassifier(_knownTypes);
            var sourceType = _knownTypes.ICollectionType.Construct(intType);
            var entry = new MappingDefinition("Test", sourceType, null!, null);

            var result = classifier.ClassifyCollectionType(entry.Type);

            Assert.True(result.IsEnumerable);
            Assert.False(result.IsType);
            Assert.True(result.IsCollection);
            Assert.Equal(CollectionKind.List, result.CollectionKind);
            Assert.Equal(intType, result.ElementsType, SymbolEqualityComparer.Default);
        }

        [Fact]
        public void HashSet()
        {
            var intType = _compilation.GetSpecialType(SpecialType.System_Int32);

            var classifier = new CollectionClassifier(_knownTypes);
            var sourceType = _knownTypes.HashSetType.Construct(intType);
            var entry = new MappingDefinition("Test", sourceType, null!, null);

            var result = classifier.ClassifyCollectionType(entry.Type);

            Assert.True(result.IsEnumerable);
            Assert.True(result.IsType);
            Assert.True(result.IsCollection);
            Assert.Equal(CollectionKind.HashSet, result.CollectionKind);
            Assert.Equal(intType, result.ElementsType, SymbolEqualityComparer.Default);
        }

        [Fact]
        public void IDictionary()
        {
            var intType = _compilation.GetSpecialType(SpecialType.System_Int32);

            var classifier = new CollectionClassifier(_knownTypes);
            var sourceType = _knownTypes.IDictionaryType.Construct(intType, intType);
            var elementType = _knownTypes.KeyValueType.Construct(intType, intType);
            var entry = new MappingDefinition("Test", sourceType, null!, null);

            var result = classifier.ClassifyCollectionType(entry.Type);

            Assert.True(result.IsEnumerable);
            Assert.False(result.IsType);
            Assert.True(result.IsCollection);
            Assert.Equal(CollectionKind.Dictionary, result.CollectionKind);
            Assert.Equal(elementType, result.ElementsType, SymbolEqualityComparer.Default);
        }

        [Fact]
        public void Dictionary()
        {
            var intType = _compilation.GetSpecialType(SpecialType.System_Int32);

            var classifier = new CollectionClassifier(_knownTypes);
            var sourceType = _knownTypes.DictionaryType.Construct(intType, intType);
            var elementType = _knownTypes.KeyValueType.Construct(intType, intType);
            var entry = new MappingDefinition("Test", sourceType, null!, null);

            var result = classifier.ClassifyCollectionType(entry.Type);

            Assert.True(result.IsEnumerable);
            Assert.True(result.IsType);
            Assert.True(result.IsCollection);
            Assert.Equal(CollectionKind.Dictionary, result.CollectionKind);
            Assert.Equal(elementType, result.ElementsType, SymbolEqualityComparer.Default);
        }

        [Fact]
        public void Collection()
        {
            var intType = _compilation.GetSpecialType(SpecialType.System_Int32);

            var classifier = new CollectionClassifier(_knownTypes);
            var sourceType = _knownTypes.CollectionType.Construct(intType);
            var entry = new MappingDefinition("Test", sourceType, null!, null);

            var result = classifier.ClassifyCollectionType(entry.Type);

            Assert.True(result.IsEnumerable);
            Assert.True(result.IsType);
            Assert.True(result.IsCollection);
            Assert.Equal(CollectionKind.Collection, result.CollectionKind);
            Assert.Equal(intType, result.ElementsType, SymbolEqualityComparer.Default);
        }

        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}
