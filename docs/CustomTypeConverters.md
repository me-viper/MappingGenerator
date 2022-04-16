# Custom Type Converters

You can provide custom type converters by defining function:

```csharp
<DESTINATION-TYPE> Convert<ANY-SUFFIX>(<SOURCE-TYPE>)
{}
```

**Note**. Custom type conversion will be used for all mapping of type `<SOURCE-TYPE>` to `<DESTINATION-TYPE>`.

```csharp
public class Source
{
    public string Number { get; set; }
}

public class Destination
{
    public int Number { get; set; }
}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{ 
    private int Convert(string source)
    {
        return int.Parse(source);
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

        // Custom type converter called.
        result.Number = Convert(source.Number);

        return result;
    }
}
```
