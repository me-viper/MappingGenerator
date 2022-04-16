using MappingGenerator.Tests.Common;

using System;
using System.Collections.Generic;

using Talk2Bits.MappingGenerator.Abstractions;

using Xunit;

namespace MappingGenerator.Tests.Dictionary
{
    public class DictionaryTests
    {
        [Fact]
        public void Simple()
        {
            var source = new Source<Dictionary<int, string>>
            {
                Value = new()
                {
                    { 1, "1" },
                    { 2, "2" },
                    { 3, "3" },
                }
            };

            var expected = new Destination<Dictionary<int, string>>
            {
                Value = new()
                {
                    { 1, "1" },
                    { 2, "2" },
                    { 3, "3" },
                }
            };

            var mapper = new Mapper();
            var result = mapper.Map(source);

            Assert.False(ReferenceEquals(expected.Value, result.Value));
            Assert.Equal(expected.Value, result.Value);
        }

        [Fact]
        public void KeyAndValueMapper()
        {
            var source = new Source<SK, SV>
            {
                Value = new()
                {
                    { new SK { Key = 1 }, new SV { Text = "1" } },
                    { new SK { Key = 2 }, new SV { Text = "2" } },
                    { new SK { Key = 3 }, new SV { Text = "3" } },
                }
            };

            var expected = new Destination<DK, DV>
            {
                Value = new()
                {
                    { new DK { Key = 1 }, new DV { Text = "1" } },
                    { new DK { Key = 2 }, new DV { Text = "2" } },
                    { new DK { Key = 3 }, new DV { Text = "3" } },
                }
            };

            var mapper = new Mapper();
            var result = mapper.Map(source);

            Assert.Equal(expected.Value, result.Value);
        }

        [Fact]
        public void KeyMapper()
        {
            var source = new Source<SK, string>
            {
                Value = new()
                {
                    { new SK { Key = 1 }, "1" },
                    { new SK { Key = 2 }, "2" },
                    { new SK { Key = 3 }, "3" },
                }
            };

            var expected = new Destination<DK, string>
            {
                Value = new()
                {
                    { new DK { Key = 1 }, "1" },
                    { new DK { Key = 2 }, "2" },
                    { new DK { Key = 3 }, "3" },
                }
            };

            var mapper = new Mapper();
            var result = mapper.Map(source);

            Assert.Equal(expected.Value, result.Value);
        }
        [Fact]
        public void ValueMapper()
        {
            var source = new Source<int, SV>
            {
                Value = new()
                {
                    { 1, new SV { Text = "1" } },
                    { 2, new SV { Text = "2" } },
                    { 3, new SV { Text = "3" } },
                }
            };

            var expected = new Destination<int, DV>
            {
                Value = new()
                {
                    { 1, new DV { Text = "1" } },
                    { 2, new DV { Text = "2" } },
                    { 3, new DV { Text = "3" } },
                }
            };

            var mapper = new Mapper();
            var result = mapper.Map(source);

            Assert.Equal(expected.Value, result.Value);
        }
    }

    public record SK { public int Key { get; set; } }
    public record SV { public string? Text { get; set; } }

    public record DK { public int Key { get; set; } }
    public record DV { public string? Text { get; set; } }

    public record Source<TKey, TValue>
        where TKey : notnull
    {
        public Dictionary<TKey, TValue> Value { get; set; } = default!;
    }

    public record Destination<TKey, TValue>
        where TKey : notnull
    {
        public Dictionary<TKey, TValue> Value { get; set; } = default!;
    }

    [MappingGenerator(typeof(SK), typeof(DK))]
    [MappingGenerator(typeof(SV), typeof(DV))]
    [MappingGenerator(typeof(KeyValuePair<SK, SV>), typeof(KeyValuePair<DK, DV>))]
    [MappingGenerator(typeof(KeyValuePair<SK, string>), typeof(KeyValuePair<DK, string>))]
    [MappingGenerator(typeof(KeyValuePair<int, SV>), typeof(KeyValuePair<int, DV>))]
    [MappingGenerator(typeof(Source<SK, SV>), typeof(Destination<DK, DV>))]
    [MappingGenerator(typeof(Source<int, SV>), typeof(Destination<int, DV>))]
    [MappingGenerator(typeof(Source<SK, string>), typeof(Destination<DK, string>))]
    [MappingGenerator(typeof(Source<Dictionary<int, string>>), typeof(Destination<Dictionary<int, string>>))]
    public partial class Mapper
    { }
}
