# Nullable Mappings

MappingGenerator respects nullable annotations.

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
        result.Value = source.Value == null ? throw new MappingNullException(typeof(Source), "Value", typeof(Destination), "Value") : source.Value;
        
        return result;
    }
}

```

Custom mapper with nullable source:

```csharp
public class SourceInner { public string? InnerText { get; set; } }
public class DestinationInner { public string? InnerText { get; set; } }

public class Source { public SourceInner? Value { get; set; } }

// Destination.Value is not nullable!
public class Destination { public DestinationInner Value { get; set; } }

[MappingGenerator(typeof(Source<SourceInner?>), typeof(Destination<DestinationInner>))]
public partial class CustomNullableMapper
{
    private IMapper<SourceInner?, DestinationInner> _nullToDefaultMapper = new NullToDefaultMapper();

    // Return predefined result if source is null.
    private class NullToDefaultMapper : IMapper<SourceInner?, DestinationInner>
    {
        public DestinationInner Map(SourceInner? source)
        {
            if (source == null)
                return new DestinationInner { InnerText = "Default" };

            return new DestinationInner { InnerText = source.InnerText };
        }
    }
}
```

Will produce the following generated code (removed redundant parts and added comments for brevity):

```csharp
partial class CustomNullableMapper : IMapper<Source<SourceInner?>, Destination<DestinationInner>>
{
    public Destination<DestinationInner> Map(Source<SourceInner?> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var result = CreateDestination();

        // Inner mapper is fine with source = null, no additional code needed.
        result.Value = this._nullToDefaultMapper.Map(source.Value);
        
        return result;
    }
}
```

Custom mapper with nullable source and destination:

```csharp
public class SourceInner { public string? InnerText { get; set; } }
public class DestinationInner { public string? InnerText { get; set; } }

public class Source { public SourceInner? Value { get; set; } }

// Destination.Value is not nullable!
public class Destination { public DestinationInner Value { get; set; } }

[MappingGenerator(typeof(Source<SourceInner?>), typeof(Destination<DestinationInner>))]
public partial class CustomNullableMapper
{
    private IMapper<SourceInner?, DestinationInner?> _nullToDefaultMapper = new NullToDefaultMapper();

    // Return null if source is null.
    private class NullToDefaultMapper : IMapper<SourceInner?, DestinationInner?>
    {
        public DestinationInner? Map(SourceInner? source)
        {
            if (source == null)
                return null;

            return new DestinationInner { InnerText = source.InnerText };
        }
    }
}
```

Will produce the following generated code (removed redundant parts and added comments for brevity):

```csharp
partial class CustomNullableMapper : IMapper<Source<SourceInner?>, Destination<DestinationInner>>
{
    public Destination<DestinationInner> Map(Source<SourceInner?> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var result = CreateDestination();

        // Inner mapper is fine with source = null but can return null.
        result.Value = this._nullToDefaultMapper.Map(source.Value) 
            ?? throw new MappingNullException(_nullToDefaultMapper.GetType(), typeof(Destination<DestinationInner>), "Value");
        
        return result;
    }
}
```
