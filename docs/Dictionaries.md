# Dictionaries

MappingGenerator supports mapping of dictionaries with the following types:

* `IDictionary<TKey, TValue>`
* `Dictionary<TKey, TValue>`

```csharp
public record Source<TKey, TValue>
    where TKey : notnull
{
    public Dictionary<TKey, TValue> Value { get; set; } = default!;
}

public record Destination<TKey, TValue>
    where TKey : notnull
{
    public Dictionary<TKey, TValue> Value { get; set; } = default!;
}

[MappingGenerator(typeof(Source<int, string>), typeof(Destination<int, string>))]
public partial class Mapper
{ }

```

Generated code (removed redundant parts and added comments for brevity):

```csharp
public Destination<int, string> Map(Source<int, string> source)
{
    if (source == null)
        throw new ArgumentNullException(nameof(source));

    var result = CreateDestination();

    if (result.Value == null)
        result.Value = CollectionsHelper.CopyToNewDictionary< int ,  string >(source.Value);
    else
        CollectionsHelper.CopyTo<System.Collections.Generic.KeyValuePair<int, string>>(source.Value, result.Value);

    return result;
}
```

## Complex mapping

If you need to do complex dictionary mapping (key and/or value) you need to inform MappingGenerator to generate mappings for corresponding `KeyValuePair`:

```csharp
public record SK { public int Key { get; set; } }
public record SV { public string? Text { get; set; } }

public record DK { public int Key { get; set; } }
public record DV { public string? Text { get; set; } }

public record Source<TKey, TValue>
    where TKey : notnull
{
    public Dictionary<TKey, TValue> Value { get; set; } = default!;
}

public record Destination<TKey, TValue>
    where TKey : notnull
{
    public Dictionary<TKey, TValue> Value { get; set; } = default!;
}

[MappingGenerator(typeof(SK), typeof(DK))]
[MappingGenerator(typeof(SV), typeof(DV))]
// Map both key and value.
[MappingGenerator(typeof(KeyValuePair<SK, SV>), typeof(KeyValuePair<DK, DV>))]
[MappingGenerator(typeof(Source<SK, SV>), typeof(Destination<DK, DV>))]
// Map key only.
[MappingGenerator(typeof(KeyValuePair<SK, string>), typeof(KeyValuePair<DK, string>))]
[MappingGenerator(typeof(Source<SK, string>), typeof(Destination<DK, string>))]
// Map values only.
[MappingGenerator(typeof(KeyValuePair<int, SV>), typeof(KeyValuePair<int, DV>))]
[MappingGenerator(typeof(Source<int, SV>), typeof(Destination<int, DV>))]
public partial class Mapper
{ }
```

Generated code (removed redundant parts and added comments for brevity):

```csharp
partial class Mapper : IMapper<SK, DK>
{
    public DK Map(SK source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var result = CreateDestination();
        result.Key = source.Key;
        return result;
    }
}

partial class Mapper : IMapper<SV, DV>
{
    public DV Map(SV source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var result = CreateDestination();
        result.Text = source.Text;
        return result;
    }
}

partial class Mapper : IMapper<KeyValuePair<SK, SV>, KeyValuePair<DK, DV>>
{
    public KeyValuePair<DK, DV> Map(KeyValuePair<SK, SV> source)
    {
        KeyValuePair<DK, DV> CreateDestination()
        {
            return new KeyValuePair<DK, DV>(this.mapper.Map(source.Key), this.mapper1.Map(source.Value))
            {};
        }

        var result = CreateDestination();
        return result;
    }
}

partial class Mapper : IMapper<KeyValuePair<SK, string>, KeyValuePair<DK, string>>
{
    public KeyValuePair<DK, string> Map(KeyValuePair<SK, string> source)
    {
        KeyValuePair<DK, string> CreateDestination()
        {
            return new KeyValuePair<DK, string>(this.mapper.Map(source.Key), source.Value)
            {};
        }

        var result = CreateDestination();
        return result;
    }
}

partial class Mapper : IMapper<KeyValuePair<int, SV>, KeyValuePair<int, DV>>
{
    public KeyValuePair<int, DV> Map(KeyValuePair<int, SV> source)
    {
        KeyValuePair<int, DV> CreateDestination()
        {
            return new KeyValuePair<int, DV>(source.Key, this.mapper1.Map(source.Value))
            {};
        }

        var result = CreateDestination();
        return result;
    }
}

partial class Mapper : IMapper<Source<SK, SV>, Destination<DK, DV>>
{
    public Destination<DK, DV> Map(Source<SK, SV> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var result = CreateDestination();

        if (result.Value == null)
            result.Value = CollectionsHelper.CopyToNewDictionary<SK, SV, DK, DV>(source.Value, p => mapper2.Map(p));
        else
            CollectionsHelper.CopyTo<KeyValuePair<SK, SV>, KeyValuePair<DK, DV>>(source.Value, result.Value, p => mapper2.Map(p));
        
        return result;
    }
}

partial class Mapper : IMapper<Source<int, SV>, Destination<int, DV>>
{
    public Destination<int, DV> Map(Source<int, SV> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var result = CreateDestination();

        if (result.Value == null)
            result.Value = CollectionsHelper.CopyToNewDictionary< int , SV,  int , DV>(source.Value, p => mapper3.Map(p));
        else
            CollectionsHelper.CopyTo<KeyValuePair<int, SV>, KeyValuePair<int, DV>>(source.Value, result.Value, p => mapper3.Map(p));
        
        return result;
    }
}

partial class Mapper : IMapper<Source<SK, string>, Destination<DK, string>>
{
    public Destination<DK, string> Map(Source<SK, string> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var result = CreateDestination();

        if (result.Value == null)
            result.Value = CollectionsHelper.CopyToNewDictionary<SK,  string , DK,  string >(source.Value, p => mapper4.Map(p));
        else
            CollectionsHelper.CopyTo<KeyValuePair<SK, string>, KeyValuePair<DK, string>>(source.Value, result.Value, p => mapper4.Map(p));
        
        return result;
    }
}

partial class Mapper
{
    private IMapper<SK, DK> mapper;
    private IMapper<SV, DV> mapper1;
    private IMapper<KeyValuePair<SK, SV>, KeyValuePair<DK, DV>> mapper2;
    private IMapper<KeyValuePair<int, SV>, KeyValuePair<int, DV>> mapper3;
    private IMapper<KeyValuePair<SK, string>, KeyValuePair<DK, string>> mapper4;
    
    public Mapper()
    {
        this.mapper = (IMapper<SK, DK>)this;
        this.mapper1 = (IMapper<SV, DV>)this;
        this.mapper2 = (IMapper<KeyValuePair<SK, SV>, KeyValuePair<DK, DV>>)this;
        this.mapper3 = (IMapper<KeyValuePair<int, SV>, KeyValuePair<int, DV>>)this;
        this.mapper4 = (IMapper<KeyValuePair<SK, string>, KeyValuePair<DK, string>>)this;
    }
}
```
