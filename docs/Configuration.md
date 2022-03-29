# Configuration

## Keeping eye on missing mappings

You can control how MappingGenerator behaves if it was not able to map everything in destination object with MissingMappingBehavior parameter. Options are:

* **Warning** (Default). Produce compilation warning.
* **Ignore**. Do nothing.
* **Error**. Produce compilation error.

For example the following code will produce compilation error because it can't find source for `B.Val`:

```csharp

public class A {}

public class B 
{ 
    public string Val { get; set; }
}

[MappingGenerator(typeof(A), typeof(B), MissingMappingBehavior = MissingMappingBehavior.Error)]
public partial class Mapper 
{
}
```

Compilation error:

`Mapping generator 'Mapper': Failed to resolve mapping for type 'B' property 'Val'.`


## Naming mappers

## Mapping constructor generation options

## Mapping implementation generation options


## Ignore destination property

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
[MappingGeneratorPropertyIgnore(nameof(Destination.BigNumber))]
public partial class Mapper
{ }
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

        // No mapping for BigNumber property.

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```

## Override property matching behavior

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
[MappingGeneratorPropertyMapping(nameof(Source.SourceText), nameof(Destination.DestinationText))]
public partial class Mapper
{ }
```

Generated code (removed redundant parts and added comments for brevity):

```csharp
partial class Mapper : IMapper<Source, Destination>
{
    public Mapper()
    {
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

        result.Number = source.Number;

        // SourceText mapped to DestinationText.
        result.DestinationText = source.SourceText;

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```
