using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MappingGenerator.Tests.Common
{
    public record Source<T>
    {
        public T Value { get; set; } = default!;
    };

    public record Source<T1, T2> : Source<T1>
    {
        public T2 Value2 { get; set; } = default!;
    }

    public record Destination<T>
    {
        public T Value { get; set; } = default!;
    }

    public record Destination<T1, T2> : Destination<T1>
    {
        public T2 Value2 { get; set; } = default!;
    }

    public record SourceInner
    {
        public static SourceInner Default => new() { InnerText = "InnerText", InnerNumber = 100 };

        public int InnerNumber { get; set; }

        public string? InnerText { get; set; }
    }

    public record DestinationInner
    {
        public static DestinationInner Default => new() { InnerText = "InnerText", InnerNumber = 100 };

        public int InnerNumber { get; set; }

        public string? InnerText { get; set; }
    }

    public record DestinationInitOnly<T1, T2> : Destination<T1>
    {
        public T2 Value2 { get; init; } = default!;
    }

    public record DestinationConstructor<T1, T2> : Destination<T1>
    {
        public DestinationConstructor(T2 value2)
        {
            Value2 = value2;
        }

        public T2 Value2 { get; } = default!;
    }

    public record DestinationWithCollection<T1, T2> : Destination<T1>
        where T2 : IEnumerable<T1>, new()
    {
        public T2 Value2 { get; } = new T2();
    }

    public record DestinationWithCollection<T1, T2, T3> : Destination<T1>
        where T2 : IEnumerable<T3>, new()
    {
        public T2 Value2 { get; } = new T2();
    }
}
