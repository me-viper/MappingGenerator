using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MappingGenerator.Tests.Common
{
    public interface IDestination<T>
    {
        T? GetValue();
    }

    public record Source<T>
    {
        public T Value { get; set; } = default!;
    };

    public record Source<T1, T2> : Source<T1>
    {
        public T2 Value2 { get; set; } = default!;
    }

    public record Destination<T> : IDestination<T>
    {
        public T Value { get; set; } = default!;

        public T GetValue() => Value;
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

    public record DestinationReadOnly<T> : IDestination<T>
    {
        public T Value { get; } = default!;

        public T GetValue() => Value;
    }

    public record DestinationInitOnly<T> : IDestination<T>
    {
        public T Value { get; init; } = default!;

        public T GetValue() => Value;
    }

    public record DestinationInitOnly<T1, T2> : Destination<T1>
    {
        public T2 Value2 { get; init; } = default!;
    }

    public record DestinationConstructor<T> : IDestination<T>
    {
        public DestinationConstructor(T value)
        {
            Value = value;
        }

        public T Value { get; } = default!;

        public T GetValue() => Value;
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
