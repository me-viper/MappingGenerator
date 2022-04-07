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
partial class Mapper : IMapper<Source, Destination>
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

    partial void AfterMap(Source source, Destination result);
}
```

Usage:

```csharp
var source = new Source[] { ... };

var mapper = new Mapper(source);
IEnumerable<Destination> enumerable = mapper.Map(source);
Destination[] array = mapper.ToArray(source);
List<Destination> list = mapper.ToList(source);
IList<Destination> ilist = mapper.ToList(source);
Collection<Destination> collection = mapper.ToCollection(source);
HashSet<Destination> collection = mapper.ToHashSet(source);

```

## Nested collections mappings behavior

When mapping enumerable properties the following rules apply:

* If source is `null` MappingGenerator will map empty collection.
* If destination is read-only property:
  * If destination implements `ICollection` and is `null` nothing will happen.
  * If destination implements `ICollection` and is not `null`, it's cleared and will be repopulated with mapped elements of source.
* If destination is read-write property:
  * If destination implements `ICollection` and is `null` it will be replaced with new `List<T>` instance with mapped elements of source.
  * If destination implements `ICollection` and is not `null`, it's cleared and will be repopulated with mapped elements of source.
  * If destination is `IEnumerable` it will be replaced with new `List<T>` instance with mapped elements of source.
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

    public IEnumerable<C> ReadOnly { get; set; } = new List<C>();

    public IEnumerable<A> Collection { get; set; } = new List<A>();

    public HashSet<A> HashSet { get; set; } = new HashSet<A>();

    public IEnumerable<long> ExplicitCast { get; set; } = new List<long>();

    public IEnumerable<D> Covariation { get; set; } = new List<D>();
}

public class Destination
{
    public IEnumerable<B> Nested { get; set; } = default!;

    public IEnumerable<C> Simple { get; set; } = default!;

    public List<C> ReadOnly { get; } = default!;

    public ICollection<B> Collection { get; set; } = default!;

    public HashSet<A> HashSet { get; set; } = new HashSet<A>();

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
        result.Nested = this.aBMapper.ToList(source.Nested);
        
        // Destination is writable IEnumerable.
        result.Simple = CollectionsHelper.CopyToNewList<C, List<C>>(source.Simple);
        
        // Destination is read-only ICollection.
        CollectionsHelper.CopyTo<C>(source.ReadOnly, result.ReadOnly);

        // Destination is read-write ICollection.
        if (result.Collection == null)
            result.Collection = CollectionsHelper.CopyToNewList<A, B>(source.Collection, p => aBMapper.Map(p));
        else
            CollectionsHelper.CopyTo<A, B>(source.Collection, result.Collection, p => aBMapper.Map(p));

        // Destination is read-write HashSet.
        if (result.HashSet == null)
            result.HashSet = CollectionsHelper.CopyToNewHashSet<A>(source.HashSet);
        else
            CollectionsHelper.CopyTo<A>(source.HashSet, result.HashSet);
        
        // Destination required explicit cast.
        if (result.ExplicitCast == null)
            result.ExplicitCast = CollectionsHelper.CopyToNewList<long, int>(source.ExplicitCast, static p => (int)p);
        else
            CollectionsHelper.CopyTo<long, int>(source.ExplicitCast, result.ExplicitCast, static p => (int)p);
        
        // IEnumerable<D> has implicit cast to ICollection<C> because D is child class of C.
        if (result.Covariation == null)
            result.Covariation = CollectionsHelper.CopyToNewList<C>(source.Covariation);
        else
            CollectionsHelper.CopyTo<C>(source.Covariation, result.Covariation);
        
        AfterMap(source, result);
        return result;
    }

    partial void AfterMap(Source source, Destination result);
}
```
