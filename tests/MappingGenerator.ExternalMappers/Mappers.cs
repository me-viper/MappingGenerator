using Talk2Bits.MappingGenerator.Abstractions;

namespace MappingGenerator.ExternalMappers
{
    public class ExternalSource
    {
        public string Text { get; set; } = default!;
    }

    public class ExternalDestination
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