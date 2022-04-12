using Talk2Bits.MappingGenerator.Abstractions;

namespace MappingGenerator.ExternalMappers
{
    public record ExternalSource
    {
        public string Text { get; set; } = default!;
    }

    public record ExternalDestination
    {
        public string Text { get; set; } = default!;

        public string ExternalText { get; set; } = default!;
    }

    [MappingGenerator(typeof(ExternalSource), typeof(ExternalDestination))]
    public partial class ExternalMapper
    {
        private string MapExternalText(ExternalSource source)
        {
            return $"External{source.Text}";
        }
    }
}