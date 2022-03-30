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

## Nested collections mappings behavior

When mapping enumerable properties the following rules apply:

* If source is `null` MappingGenerator will map empty collection.
* If destination implements `ICollection` (can be readonly property), it's cleared and will be repopulated with mapped elements of source.
* If destination is `IEnumerable` (must have setter) it will be replaced with new `List<T>` instance with mapped elements of source.
* Constructor parameters and init-only properties always initialized new `List<T>` instance with mapped elements of source.

The following sample demonstrates more MappingGenerator features:

```csharp
public class A { }

public class B { }

public class C { }

public class D : C { }

public class Source
{
    public IEnumerable<A> Nested { get; set; } = new List<A>();

    public IEnumerable<C> Simple { get; set; } = new List<C>();

    public IEnumerable<A> Collection { get; set; } = new List<A>();

    public IEnumerable<long> ExplicitCast { get; set; } = new List<long>();

    public IEnumerable<D> Covariation { get; set; } = new List<D>();
}

public class Destination
{
    public IEnumerable<B> Nested { get; set; } = default!;

    public IEnumerable<C> Simple { get; set; } = default!;

    public ICollection<B> Collection { get; set; } = default!;

    public ICollection<int> ExplicitCast { get; set; } = default!;

    public ICollection<C> Covariation { get; set; } = default!;
}

[MappingGenerator(typeof(A), typeof(B))]
public partial class ABMapper
{
}

[MappingGenerator(typeof(Source), typeof(Destination))]
public partial class Mapper
{
}
```

Generated code (removed redundant parts and added comments for brevity):

```csharp
// Implementation omitted.
partial class ABMapper
{}

partial class Mapper : IMapper<Source, Destination>
{
    private IMapper<A, B> aBMapper;

    public Mapper(IMapper<A, B> aBMapper)
    {
        if (aBMapper == null)
            throw new ArgumentNullException(nameof(aBMapper));
        this.aBMapper = aBMapper;
    }

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

        // Reused existing mapper A => B.
        result.Nested = ((IMapper<IEnumerable<A>, IEnumerable<B>>)this.aBMapper).Map(source.Nested);
        
        // Destination is writable IEnumerable.
        result.Simple = CollectionsHelper.CopyToNew<C, List<C>>(source.Simple);
        
        // Destination is ICollection.
        CollectionsHelper.CopyTo<A, B>(source.Collection, result.Collection, aBMapper);
        
        // Generated required explicit cast.
        CollectionsHelper.CopyTo< long ,  int >(source.ExplicitCast, result.ExplicitCast, static p => ( int )p);
        
        // IEnumerable<D> has implicit cast to ICollection<C> because D is child class of C.
        CollectionsHelper.CopyTo<C>(source.Covariation, result.Covariation);
        
        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```
