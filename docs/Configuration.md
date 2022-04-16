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

You can name generated mappers by adding `MappingGenerator.Name` parameter:

```csharp
[MappingGenerator(typeof(Source), typeof(Destination), Name = "My")]
public partial class Mapper
{
}
```

**Important**. `Name` must be valid C# identifier.

This might be useful if anchor class contains more than one mapper and you want to avoid naming conflicts (for more information see [Multiple Mappings](./MultipleMappers.md) section).

Naming a mapper has the following effects:

* `CreateDestination` method => `<NAME>CreateDestination`
* `AfterMap` method => `<NAME>AfterMap`
* `Map<PROPERTY>` methods => `<NAME>Map<PROPERTY>`
* Fields and constructor parameters naming

Generated code (removed redundant parts and added comments for brevity):

```csharp
partial class Mapper : IMapper<Source, Destination>
{
    private Destination MyCreateDestination(Source source)
    {
        ...
    }

    public Destination Map(Source source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        var result = MyCreateDestination(source);

        ...

        MyAfterMap(source, ref result);
        return result;
    }

    private partial void MyAfterMap(Source source, ref Destination result);
}
```

## Mapping constructor generation options

You can control accessibility of constructor generated for anchor class with `MappingGenerator.ConstructorAccessibility` parameter which can have the following values:

* **Public**. Constructor will be `public` (Default).
* **Private**. Constructor will be `private`.
* **PrivateProtected**. Constructor will be `private protected`.
* **Protected**. Constructor will be `protected`.
* **Internal**. Constructor will be `internal`.
* **InternalProtected**. Constructor will be `protected internal`.
* **Suppress**. No constructor will be generated.

**Important**. If you set `MappingGenerator.ConstructorAccessibility = ConstructorAccessibility.Suppress` it's up to you to initialize all generated fields otherwise you will get runtime `NullReferenceException` errors.

## Mapping implementation generation options

You can control how generated mappers implement `IMapper<Source, Destination>` interface with `MappingGenerator.ImplementationType` parameter which can have the following options:

* **Implicit**. Implicit implementation.
* **Explicit**. Explicit implementation.

**Note**. `IMapper<IEnumerable<Source>, ...>` interfaces always have explicit implementation.

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

        return result;
    }
}
```

**Note**. By default `MappingGeneratorPropertyIgnore` will be applied to all mappers within anchor class. If you want it to be applied only to specific mapper use `AppliesTo` parameter:

```csharp
[MappingGenerator(typeof(A), typeof(B))]
[MappingGenerator(typeof(C), typeof(D), Name = "B2DMapper")]
// 'IgnoreAll' property will be ignored both for A => B and C => D mappers.
[MappingGeneratorPropertyIgnore(nameof("IgnoreAll"))]
// 'Some' property will be ignored only for C => D mapper.
[MappingGeneratorPropertyIgnore(nameof("IgnoreSome"), AppliesTo = "B2DMapper")]
public partial class Mapper
{ }
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

        return result;
    }
}
```

**Note**. By default `MappingGeneratorPropertyMapping` will be applied to all mappers within anchor class. If you want it to be applied only to specific mapper use `AppliesTo` parameter:

```csharp
[MappingGenerator(typeof(A), typeof(B))]
[MappingGenerator(typeof(C), typeof(D), Name = "B2DMapper")]
// 'SourceVal' property will be mapped to 'DestVal' property for both for A => B and C => D mappers.
[MappingGeneratorPropertyMapping("SourceVal", "DestVal")]
// 'CVal' property will be mapped to 'DVal' property only for C => D mapper.
[MappingGeneratorPropertyMapping("CVal", "DVal", AppliesTo = "B2DMapper")]
public partial class Mapper
{ }
```
