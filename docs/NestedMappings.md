# Nested mappings

MappingGenerator can detect nested mappings and reuse other generated mappers to do the mapping:

```csharp
public class InnerSource
{
    public string InnerText { get; set; }
}

public class InnerDestination
{
    public string InnerText { get; set; }
}

public class Source
{
    public string Text { get; set; }
    public InnerSource Inner { get; set; }
}

public class Destination
{
    public string Text { get; set; }
    public InnerDestination Inner { get; set; }
}

[MappingGenerator(typeof(InnerSource), typeof(InnerDestination))]
public partial class InnerMapper
{}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{}
```

Generated code (removed redundant parts and added comments for brevity):

```csharp
partial class InnerMapper : IMapper<InnerSource, InnerDestination>
{
    public InnerDestination Map(InnerSource source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var result = CreateDestination();
        result.InnerText = source.InnerText;
        AfterMap(source, result);
        return result;
    }
}

partial class Mapper : IMapper<Source, Destination>
{
    // Mapper InnerSource => InnerDestination.
    private IMapper<InnerSource, InnerDestination> innerMapper;

    public Mapper(IMapper<InnerSource, InnerDestination> innerMapper)
    {
        if (innerMapper == null)
            throw new ArgumentNullException(nameof(innerMapper));

        this.innerMapper = innerMapper;
    }

    public Destination Map(Source source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var result = CreateDestination();
        result.Text = source.Text;

        // Reusing InnerSource => InnerDestination for nested mapping.
        result.Inner = this.innerMapper.Map(source.Inner);
        
        AfterMap(source, result);
        return result;
    }
}

```
