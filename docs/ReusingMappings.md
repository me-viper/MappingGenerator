# Reusing generated mappings

MappingGenerator will reuse other generated mappings.

```csharp
public class InnerSource
{
    public string InnerText { get; set; }
}

public class Source
{
    public InnerSource A { get; set; }
    public InnerSource B { get; set; }
}

public class InnerDestination
{
    public string InnerText { get; set; }
}

public class Destination
{
    public InnerDestination A { get; set; }
    public InnerDestination B { get; set; }
}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{
}

[MappingGenerator(typeof(InnerSource), typeof(InnerDestination))]
public partial class InnerMapper
{
}
```

Generated code (removed redundant parts and added comments for brevity):

```csharp
// Mapping for InnerSource => InnerDestination
partial class InnerMapper : IMapper<InnerSource, InnerDestination>
{
    public InnerDestination Map(InnerSource source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        var result = CreateDestination(source);

        result.InnerText = source.InnerText;

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}

partial class Mapper : IMapper<Source, Destination>
{
    private IMapper<InnerSource, InnerDestination> innerMapper;

    // Getting InnerMapper.
    public Mapper(IMapper<InnerSource, InnerDestination> innerMapper)
    {
        if (innerMapper == null)
            throw new ArgumentNullException(nameof(innerMapper));

        this.innerMapper = innerMapper;
    }

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

        // Reusing InnerMapper.
        result.A = this.innerMapper(source.A);
        result.B = this.innerMapper(source.B);

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```
