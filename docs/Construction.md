# Construction

## Mapping constructor parameters

MappingGenerator will try to find destination object's constructor with the most parameters that can be mapped from source object.

**Note**. If MappingGenerator fails to find appropriate constructor if will produce compilation error.

```csharp
public class Source
{
    public int Number { get; set; }

    public string Text { get; set; }
}

public class Destination
{

    public Destination(string text)
    {
        Text = text;
    }

    public int Number { get; set; }

    public string Text { get; }
}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{
}
```

Generated code (removed redundant parts and added comments for brevity):

```csharp
partial class Mapper : IMapper<Source, Destination>
{
    private Destination CreateDestination(Source source)
    {
        // Parameter 'text' have been mapped to 'source.Text'.
        return new Destination(source.Text)
        {};
    }

    public Destination Map(Source source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        var result = CreateDestination(source);

        result.Number = source.Number;
        
        // NOTE. Mapping result.Text = source.Text is omitted
        // because MappingGenerator considers mapping was done in constructor.

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```

## Mapping init only properties

```csharp
public class Source
{
    public int Number { get; set; }

    public string Text { get; set; }
}

public class Destination
{
    public int Number { get; set; }

    public string Text { get; init; }
}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{
}
```

Generated code (removed redundant parts and added comments for brevity):

```csharp
partial class Mapper : IMapper<Source, Destination>
{
    private Destination CreateDestination(Source source)
    {
        // Init only property 'Text' have been mapped to 'source.Text'.
        return new Destination()
        { 
            Text = source.Text
        };
    }

    public Destination Map(Source source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        var result = CreateDestination(source);

        result.Number = source.Number;

        // NOTE. Mapping result.Text = source.Text is omitted
        // because MappingGenerator considers mapping have been done already.

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```

## Custom construction

You can control how destination object is constructed by defining function:

```csharp
<DESTINATION-TYPE> <MAPPER-NAME>CreateDestination(Source source)
{}
```

**Note**. If you need to do some pre-mapping logic, custom constructor is the right place for it.

```csharp
public class Source
{
    public int Number { get; set; }

    public string Text { get; set; }
}

public class Destination
{
    public Destination(string input) 
    {}

    public int Number { get; set; }

    public string Text { get; set; }
}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{ 
    private Destination CreateDestination(Source source)
    {
        return new Destination("input");
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
        
        // Your custom CreateDestination method used.
        var result = CreateDestination(source);

        result.Number = source.Number;
        result.Text = source.Text;

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```
