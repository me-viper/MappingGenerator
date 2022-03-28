# Lists and Arrays

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
