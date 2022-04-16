# Executing post mapping logic

MappingGenerator generates partial `AfterMap` method which is called after mapping is done.

```csharp
partial void <MAPPER-NAME>AfterMap(Source source, ref Destination result)
{}
```

```csharp
public class Source
{
    public int Number { get; set; }

    public string Text { get; set; }
}

public class Destination
{
    public int Number { get; set; }

    public string Text { get; set; }
}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{ 
    private partial void AfterMap(Source source, ref Destination result);
    {
        result.Number = result.Number + 100;
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
        result.Text = source.Text;

        // Will call your AfterMap method.
        AfterMap(source, ref result);
        return result;
    }

    partial void AfterMap(Source source, ref Destination result);
}
```
