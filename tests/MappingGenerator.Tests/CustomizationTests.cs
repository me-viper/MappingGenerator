using System;

using Talk2Bits.MappingGenerator.Abstractions;

using Xunit;

namespace MappingGenerator.Tests.MappingCustomization
{
    public class CustomizationTests
    {
        [Fact]
        public void PropertyMappingCustomization()
        {
            var source = new Source { Value = "Text", Ignore = "IgnoreMe", SourceValue = "Custom" };
            var expected = new Destination { Value = "Text", Ignore = null, DestinationValue = "Custom" };

            var mapper = new CustomizedMapper();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CustomMappingMethods()
        {
            var source = new Source { Value = "Text", Ignore = "IgnoreMe", SourceValue = "Custom" };
            var expected = new Destination { Value = "CustomMapText", Ignore = null, DestinationValue = "CustomMapCustom" };

            var mapper = new MapperWithMethods();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CustomMappingConstructor()
        {
            var source = new Source { Value = "Text", Ignore = "IgnoreMe", SourceValue = "Custom" };
            var expected = new Destination("ConstructedIgnoreMe") { Value = "Text", DestinationValue = "Custom" };

            var mapper = new MapperWithConstructor();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CustomMethodsPrefix()
        {
            var source = new Source { Value = "Text", Ignore = "IgnoreMe", SourceValue = "Custom" };
            var expected = new Destination("ConstructedIgnoreMe") { Value = "CustomMapText", DestinationValue = "CustomMapCustom" };

            var mapper = new MapperWithCustomPrefix();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void AfterMap()
        {
            var source = new Source { Value = "Text", Ignore = "IgnoreMe", SourceValue = "Custom" };
            var expected = new Destination { Value = "Text", Ignore = "AfterMap", DestinationValue = "Custom" };

            var mapper = new MapperWithAfterMap();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExlicitMapper()
        {
            var source = new Source { Value = "Text", Ignore = "IgnoreMe", SourceValue = "Custom" };
            var expected = new Destination { Value = "Text", Ignore = null, DestinationValue = null };

            var mapper = (IMapper<Source, Destination>)new ExplicitMapper();
            var result = mapper.Map(source);

            Assert.Equal(expected, result);
        }
    }

    public record Source
    {
        public string Value { get; set; } = default!;

        public string? Ignore { get; set; }

        public string SourceValue { get; set; } = default!;
    }

    public record Destination
    {
        public Destination()
        {
        }

        public Destination(string constructedValue)
        {
            ConstructedValue = constructedValue;
        }

        public string Value { get; set; } = default!;

        public string? Ignore { get; set; } = default!;

        public string? DestinationValue { get; set; }

        public string? ConstructedValue { get; }
    }

    [MappingGenerator(typeof(Source), typeof(Destination))]
    [MappingGeneratorPropertyIgnore(nameof(Destination.Ignore))]
    [MappingGeneratorPropertyIgnore(nameof(Destination.Value), AppliesTo = "A")]
    [MappingGeneratorPropertyMapping(nameof(Source.SourceValue), nameof(Destination.DestinationValue))]
    [MappingGeneratorPropertyMapping(nameof(Source.SourceValue), nameof(Destination.Value), AppliesTo = "A")]
    public partial class CustomizedMapper
    { }

    [MappingGenerator(typeof(Source), typeof(Destination))]
    [MappingGeneratorPropertyIgnore(nameof(Destination.Ignore))]
    [MappingGeneratorPropertyMapping(nameof(Source.SourceValue), nameof(Destination.DestinationValue))]
    public partial class MapperWithMethods
    {
        private string MapValue(Source source)
        {
            return $"CustomMap{source.Value}";
        }

        private string MapDestinationValue(Source source)
        {
            return $"CustomMap{source.SourceValue}";
        }
    }

    [MappingGenerator(typeof(Source), typeof(Destination))]
    [MappingGeneratorPropertyIgnore(nameof(Destination.Ignore))]
    [MappingGeneratorPropertyMapping(nameof(Source.SourceValue), nameof(Destination.DestinationValue))]
    public partial class MapperWithConstructor
    {
        private Destination CreateDestination(Source source)
        {
            return new Destination($"Constructed{source.Ignore}");
        }
    }

    [MappingGenerator(typeof(Source), typeof(Destination), Name = "Ex")]
    [MappingGeneratorPropertyIgnore(nameof(Destination.Ignore), AppliesTo = "Ex")]
    [MappingGeneratorPropertyMapping(nameof(Source.SourceValue), nameof(Destination.DestinationValue), AppliesTo = "Ex")]
    public partial class MapperWithCustomPrefix
    {
        private string ExMapValue(Source source)
        {
            return $"CustomMap{source.Value}";
        }

        private string ExMapDestinationValue(Source source)
        {
            return $"CustomMap{source.SourceValue}";
        }

        private Destination ExCreateDestination(Source source)
        {
            return new Destination($"Constructed{source.Ignore}");
        }
    }

    [MappingGenerator(typeof(Source), typeof(Destination))]
    [MappingGeneratorPropertyIgnore(nameof(Destination.Ignore))]
    [MappingGeneratorPropertyMapping(nameof(Source.SourceValue), nameof(Destination.DestinationValue))]
    public partial class MapperWithAfterMap
    {
        partial void AfterMap(Source source, Destination result)
        {
            result.Ignore = "AfterMap";
        }
    }

    [MappingGenerator(typeof(Source), typeof(Destination), ImplementationType = ImplementationType.Explicit)]
    [MappingGeneratorPropertyIgnore(nameof(Destination.Ignore), nameof(Destination.DestinationValue))]
    public partial class ExplicitMapper
    { }
}
