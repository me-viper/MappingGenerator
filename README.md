# MappingGenerator

MappingGenerator is C# source generator that allows generating object mapping code on compilation stage.

Having source code for your mappings generated provides the following benefits:

* Your mappings are always up to date.
* You see code for all your mappings. Nothing is hidden.
* All debugging features are available. You can step-in to your mappings, set breakpoints etc.
* If mapping can't be done or has issues you get compiler errors rather than runtime errors.

## Table of contents

* [How do I get started?](#how-do-i-get-started)
* [Features](#features)
  * [Basic mapping](#basic-mapping)
  * [Keeping eye on missing mappings](#keeping-eye-on-missing-mappings)
  * [Ignore destination property](#ignore-destination-property)
  * [Override property matching behavior](#override-property-matching-behavior)
  * [Provide custom mapping for destination property](#provide-custom-mapping-for-destination-property)
  * [Executing post mapping logic](#executing-post-mapping-logic)
  * [Providing custom destination object constructor](#providing-custom-destination-object-constructor)
  * [Mapping constructor parameters](#mapping-constructor-parameters)
  * [Mapping init only properties](#mapping-init-only-properties)
  * [Reusing generated mappings](#reusing-generated-mappings)
  * [Custom type converters](#custom-type-converters)
  * [Open generics](#open-generics)
  * [Arrays and collections](#arrays-and-collections)

## How do I get started?

Install [Talk2Bits.MappingGenerator](https://www.nuget.org/packages/Talk2Bits.MappingGenerator) nuget package.

Define source and destination:

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
```

Define mapper class:

```csharp
[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{ }
```

Rebuild your project

Use generated mapping:

```csharp
var source = new Source();
var mapper = new Mapper();
var result = mapper.Map(source);

```

For more information check out the [getting started](https://mappinggenerator.readthedocs.io/en/latest/GettingStarted.html) guide.

## Features

### Providing custom destination object constructor

You can control have destination object is constructed by adding function:
```
<DESTINATION-TYPE> CreateDestination(Source source)
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

Generated code (removed redundant parts and added comments for clarity):

```csharp
partial class Mapper : IMapper<Source, Destination>
{
    public Mapper()
    {
    }

    public Destination Map(Source source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        
        // Your CreateDestination method used.
        var result = CreateDestination(source);

        result.Number = source.Number;
        result.Text = source.Text;

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```

### Mapping constructor parameters

MappingGenerator will try to find destination object's constructor that has parameters that can be mapped from source object.

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

Generated code (removed redundant parts and added comments for clarity):

```csharp
partial class Mapper : IMapper<Source, Destination>
{
    public Mapper()
    {
    }

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

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```

### Mapping init only properties

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

Generated code (removed redundant parts and added comments for clarity):

```csharp
partial class Mapper : IMapper<Source, Destination>
{
    public Mapper()
    {
    }

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

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```

### Reusing generated mappings

MappingGenerator will reuse other mappings.

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

Generated code (removed redundant parts and added comments for clarity):

```csharp
// Mapping for InnerSource => InnerDestination
partial class InnerMapper : IMapper<InnerSource, InnerDestination>
{
    public Mapper()
    {
    }

    private InnerDestination CreateDestination(Source source)
    {
        return new InnerDestination()
        {};
    }

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

### Custom type converters

You can provide custom type converters by defining function:

```
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

Generated code (removed redundant parts and added comments for clarity):

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

        // Custom type converter called.
        result.Number = Convert(source.Number);

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```

### Open generics

MappingGenerator is able to handle unbounded generic types (open generics). In this case your mapper type definition should have number of generic parameters corresponding to number of unbound generic parameters in both source and destinations types. 

For example:

```csharp
[MappingGenerator(typeof(Source<>), typeof(Destination<>))]
public partial class Mapper<TSource, TDestination>
{}
```

Is equivalent to `Source<TSource>` and `Destination<TDestination>`

```csharp
[MappingGenerator(typeof(Source<,>), typeof(Destination<,>))]
public partial class Mapper<TSource1, TSource2, TDestination1, TDestination2>
{}
```

Is equivalent to `Source<TSource1, TSource2>` and `Destination<TDestination1, TDestination2>`

When defining mappers for unbound generic types you need to instruct MappingGenerator how `TSource` can be converted to `TDestination` (remember, your mapping should be compilable). You can do it by providing type converter method:

```csharp
[MappingGenerator(typeof(Source<>), typeof(Destination<>))]
public partial class Mapper<TSource, TDestination>
{
    // Now mapping generator knows how to convert TSource => TDestination
    private TDestination Convert(TSource source)
    {
        // Some conversion logic.
    }
}
```

Or by adding generic constraints:

```csharp
[MappingGenerator(typeof(Source<>), typeof(Destination<>))]
public partial class Mapper<TSource, TDestination>
    where TSource : TDestination // TSource can be casted implicitly to TDestination
{
}
```

Complete example:

```csharp
public class Source<T>
{
    public T A { get; set; }
}

public class Destination<T>
{
    public T A { get; set; }
}

[MappingGenerator(typeof(Source<>), typeof(Destination<>))]
public partial class Mapper<TSource, TDestination>
{
    private TDestination Convert(TSource source)
    {
        // Some conversion logic.
    }
}
```

Generated code (removed redundant parts and added comments for clarity):

```csharp
partial class Mapper<TSource, TDestination> : IMapper<TSource, TDestination>
{
    public Mapper()
    {
    }

    private Destination<TDestination> CreateDestination(Source<TSource> source)
    {
        return new Destination<TDestination>()
        {};
    }

    public Destination<TDestination> Map(Source<TSource> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        var result = CreateDestination(source);

        // Custom type converter called.
        result.A = Convert(source.A);

        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```

### Arrays and collections

You don't need to define separate mapper for collection or array type, MappingGenerator creates them for you.

Source collection types:

* `IEnumerable<T>`
* `ICollection<T>`
* `IList<T>`
* `Collection<T>`
* `HashSet<T>`
* `List<T>`
* `Arrays`

When mapping to an existing collection, the destination collection is cleared first.

```csharp
public class Source
{
    public string Text { get; set; }
}

public class Destination
{
    public string Text { get; set; }
}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{ }
```

Generated code (removed redundant parts and added comments for clarity):

```csharp
partial class Mapper : 
    IMapper<Source, Destination>, 
    IMapper<IEnumerable<Source>, List<Destination>>, 
    IMapper<IEnumerable<Source>, HashSet<Destination>>, 
    IMapper<IEnumerable<Source>, System.Collections.ObjectModel.Collection<Destination>>, 
    IMapper<IEnumerable<Source>, Destination[]>
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
        result.Text = source.Text;
        AfterMap(source, result);
        return result;
    }

    // !!! Explicit interface implementation.
    List<Destination> IMapper<IEnumerable<Source>, List<Destination>>.Map(IEnumerable<Source> source)
    {
        source ??= Enumerable.Empty<Source>();
        return new List<Destination>(source.Select(Map));
    }

    // !!! Explicit interface implementation.
    HashSet<Destination> IMapper<IEnumerable<Source>, HashSet<Destination>>.Map(IEnumerable<Source> source)
    {
        source ??= Enumerable.Empty<Source>();
        return new HashSet<Destination>(source.Select(Map));
    }

    // !!! Explicit interface implementation.
    Collection<Destination> IMapper<IEnumerable<Source>, Collection<Destination>>.Map(IEnumerable<Source> source)
    {
        var result = ((IMapper<IEnumerable<Source>, List<Destination>>)this).Map(source);
        // Collection is wrapper over IList.
        return new Collection<Destination>(result);
    }

    // !!! Explicit interface implementation.
    Destination[] IMapper<IEnumerable<Source>, Destination[]>.Map(IEnumerable<Source> source)
    {
        source ??= Enumerable.Empty<Source>();
        return source.Select(Map).ToArray();
    }

    partial void AfterMap(Source source, Destination result);
}
```

Usage:

```csharp
var source = new Source[] { ... };
// Collection mappers require explicit cast.
var mapper = (IMapper<IEnumerable<Source>, List<Destination>)new Mapper(source);
var result = mapper.Map(source);

```
