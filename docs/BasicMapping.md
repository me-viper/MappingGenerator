# Basic mapping

```csharp
public class Source
{
    public int Number { get; set; }

    public string Text { get; set; }

    public long BigNumber { get; set; }
}

public class Destination
{
    public int Number { get; set; }

    public string Text { get; set; }

    public long BigNumber { get; set; }
}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{ }
```

Generated code (removed redundant parts and added comments for brevity):

```csharp
partial class Mapper : IMapper<Source, Destination>
{
    private Destination CreateDestination(Source source)
    {
        return new Destination()
        {};
    }

    public Destination Map(Source source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        var result = CreateDestination(source);

        // Properties are matched by name and type.
        result.Number = source.Number;
        result.Text = source.Text;

        // Explicit cast generated.
        result.BigNumber = (int)source.BigNumber;
        
        return result;
    }
}
```
