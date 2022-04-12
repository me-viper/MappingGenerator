# Reusing generated mappings

MappingGenerator will reuse other generated mappings.

**Note**. At this moment. due to significant reflection overheads only mappers generated within same assembly are considered. If you wan't to use mappers from other assemblies see section bellow.

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

## Controlling which mappers used

If MappingGenerator finds fields or properties of `IMapper<TSource, TDestination` type that are relevant for generation it will use them:


```csharp
public record A { public string Value { get; set; } = default!; }
public record B { public string Value { get; set; } = default!; }
public record C { public string Value { get; set; } = default!; }

public record Source
{
    public A Value1 { get; set; } = default!;
    public A Value2 { get; set; } = default!;
}

public record Destination
{
    public B Value1 { get; set; } = default!;
    public C Value2 { get; set; } = default!;
}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{
    // We want use specific A => B mapper.
    private IMapper<A, B> _fieldMapper = new A2B();

    // We want use specific A => C mapper.
    public IMapper<A, C> PropertyMapper { get; } = new A2C();

    private class A2B : IMapper<A, B>
    {
        public B Map(A source)
        {
            return new B { Value = source.Value };
        }
    }

    private class A2C : IMapper<A, C>
    {
        public C Map(A source)
        {
            return new C { Value = source.Value };
        }
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

        Destination CreateDestination()
        {
            return new Destination()
            {};
        }

        var result = CreateDestination();

        // Using A => B mapper.
        result.Value1 = this._fieldMapper.Map(source.Value1);

        // Using A => C mapper.
        result.Value2 = this.PropertyMapper.Map(source.Value2);

        AfterMap(source, result);
        return result;
    }
}
```

## Reusing mappers from other assemblies

You can tell MappingGenerator where to look for mappers with `MappingGeneratorIncludeAssembly` and `MappingGeneratorIncludeMapper` attributes:

```csharp
// Mapping generator will use all types implementing IMapper<TSource, TDestination>
// from External.Mappers assembly.
[assembly: MappingGeneratorIncludeAssemblyAttribute("External.Mappers") ]
```

**Important**. `External.Mappers` assembly must be referenced by current project.

```csharp
// Mapping generator will use MapperA and MapperB implementations.
[assembly: MappingGeneratorIncludeMapper(typeof(MapperA), typeof(MapperB)) ]
```
