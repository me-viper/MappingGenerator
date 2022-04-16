# Nullable Mappings

MappingGenerator tries respects nullable annotations.

If Source and Destination have same nullable annotations:

```csharp
public class Source { public string? Value { get; set; } }
public class Destination { public string? Value { get; set; } }

[MappingGenerator(typeof(Source), typeof(Destination))]
public class Mapper
{}
```

Will produce the following generated code (removed redundant parts and added comments for brevity):

```csharp
partial class Mapper : IMapper<Source, Destination>
{
    public Destination Map(Source source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        
        var result = CreateDestination();

        // Destination.Value and Source.Value are both string? type.
        result.Value = source.Value;
        
        AfterMap(source, result);
        return result;
    }
}

```

If Source and Destination have different nullable annotations:

```csharp
public class Source { public string? Value { get; set; } }

// Destination.Value is not nullable!
public class Destination { public string Value { get; set; } }

[MappingGenerator(typeof(Source), typeof(Destination))]
public class Mapper
{}
```

Will produce the following generated code (removed redundant parts and added comments for brevity):

```csharp
partial class Mapper : IMapper<Source, Destination>
{
    public Destination Map(Source source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        
        var result = CreateDestination();

        // Destination.Value is not nullable but Source.Value is.
        result.Value = source.Value == null ? throw new SourceMemberNullException(typeof(Source), "Value", typeof(Destination), "Value") : source.Value;
        
        AfterMap(source, result);
        return result;
    }
}

```
