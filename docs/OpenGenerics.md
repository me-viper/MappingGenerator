# Open (Unbounded) generics

MappingGenerator is able to handle unbounded generic types (open generics). In this case your mapper type definition should have number of generic parameters corresponding to number of unbound generic parameters in both source and destinations types.

For example:

```csharp
[MappingGenerator(typeof(Source<>), typeof(Destination<>))]
public partial class Mapper<TSource, TDestination>
{}
```

Is equivalent to `Source<TSource>` and `Destination<TDestination>`

```csharp
[MappingGenerator(typeof(Source<,>), typeof(Destination<,>))]
public partial class Mapper<TSource1, TSource2, TDestination1, TDestination2>
{}
```

Is equivalent to `Source<TSource1, TSource2>` and `Destination<TDestination1, TDestination2>`

When defining mappers for unbound generic types you need to instruct MappingGenerator how `TSource` can be converted to `TDestination` (remember, your mapping should be compilable). You can do it by providing type converter method:

```csharp
[MappingGenerator(typeof(Source<>), typeof(Destination<>))]
public partial class Mapper<TSource, TDestination>
{
    // Now mapping generator knows how to convert TSource => TDestination
    private TDestination Convert(TSource source)
    {
        // Some conversion logic.
    }
}
```

Or by adding generic constraints:

```csharp
[MappingGenerator(typeof(Source<>), typeof(Destination<>))]
public partial class Mapper<TSource, TDestination>
    where TSource : TDestination // TSource can be casted implicitly to TDestination
{
}
```

Complete example:

```csharp
public class Source<T>
{
    public T A { get; set; }
}

public class Destination<T>
{
    public T A { get; set; }
}

[MappingGenerator(typeof(Source<>), typeof(Destination<>))]
public partial class Mapper<TSource, TDestination>
{
    private TDestination Convert(TSource source)
    {
        // Some conversion logic.
    }
}
```

Generated code (removed redundant parts and added comments for brevity):

```csharp
partial class Mapper<TSource, TDestination> : IMapper<Source<TSource>, Destination<TDestination>>
{
    public Mapper()
    {
    }

    private Destination<TDestination> CreateDestination(Source<TSource> source)
    {
        return new Destination<TDestination>()
        {};
    }

    public Destination<TDestination> Map(Source<TSource> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        var result = CreateDestination(source);

        // Custom type converter called.
        result.A = Convert(source.A);

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```
