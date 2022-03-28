# Custom Mapping

You can provide custom mapping for property by adding function:

```csharp
<DESTINATION-PROPERTY-TYPE> <MAPPER-NAME>Map<DESTINATION-PROPERTY-NAME>(Source source)
{}
```

**Note**. If MappingGenerator detects there is no target in destination type to which custom mapping function can be applied to it will produce compiler warning.

```csharp
public class Source
{
    public int Number { get; set; }

    public string SourceText { get; set; }
}

public class Destination
{
    public int Number { get; set; }

    public string DestinationText { get; set; }
}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{ 
    private string MapDestinationText(Source source)
    {
        return "Custom" + source.SourceText;
    }
}
```

Generated code (removed redundant parts and added comments for brevity):

```csharp
partial class Mapper : IMapper<Source, Destination>
{
    public Destination Map(Source source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        var result = CreateDestination(source);

        result.Number = source.Number;

        // User provided mapping used.
        result.DestinationText = MapDestinationText(source);

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```

Custom mapping function for named mapper:

```csharp
public class Source
{
    public int Number { get; set; }

    public string SourceText { get; set; }
}

public class Destination
{
    public int Number { get; set; }

    public string DestinationText { get; set; }
}

[MappingGenerator(typeof(Source), typeof(Destination), Name = "My")]
public partial class Mapper
{ 
    private string MyMapDestinationText(Source source)
    {
        return "Custom" + source.SourceText;
    }
}
```
