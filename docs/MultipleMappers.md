# Multiple mappings

MappingGenerator supports generation of multiple mappers within same anchor class:

```csharp
public class A {}
public class B {}
public class C {}
public class D {}

[MappingGenerator(typeof(A), typeof(B))]
[MappingGenerator(typeof(C), typeof(D))]
public partial class Mapper
{}
```

For such cases the following rules apply:

* Each mapper instance will be generated in separate "part" of anchor class.
* If mappers have dependencies on other mappers (e.g. require constructors) all constructors will be merged into single constructor.
* Additional "part" will be generated containing constructor only.
* If mapper A in anchor class can be reused to generate mapper B, no additional dependencies will be added to the anchor class.

For example:

```csharp
public class A
{
    public string Text { get; set; }
}

public class B
{
    public string Text { get; set; }
}

public class C
{
    public A Inner { get; set; }
}

public class D
{
    public B Inner { get; set; }
}

[MappingGenerator(typeof(A), typeof(B))]
[MappingGenerator(typeof(C), typeof(D))]
partial class Mapper
{}
```

Will produce the following generated code (removed redundant parts and added comments for brevity):

```csharp
namespace MappingGenerator.SampleApp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Talk2Bits.MappingGenerator.Abstractions;

    // A => B mapper generated in separate "part" of anchor class.
    partial class Mapper : IMapper<A, B>
    {
        public B Map(A source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            B CreateDestination()
            {
                return new B()
                {};
            }

            var result = CreateDestination();
            result.Text = source.Text;
            AfterMap(source, result);
            return result;
        }

        partial void AfterMap(A source, B result);
    }
}
namespace MappingGenerator.SampleApp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Talk2Bits.MappingGenerator.Abstractions;

    // C => D mapper generated in separate "part" of anchor class.
    partial class Mapper : IMapper<C, D>, IMapper<IEnumerable<C>, List<D>>, IMapper<IEnumerable<C>, HashSet<D>>, IMapper<IEnumerable<C>, Collection<D>>, IMapper<IEnumerable<C>, D[]>
    {
        public D Map(C source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            D CreateDestination()
            {
                return new D()
                {};
            }

            var result = CreateDestination();

            // Reusing A => B mapper.
            result.Inner = this.mapper.Map(source.Inner);
            
            AfterMap(source, result);
            return result;
        }

        partial void AfterMap(C source, D result);
    }
}
namespace MappingGenerator.SampleApp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Talk2Bits.MappingGenerator.Abstractions;

    // Separate "part" containing constructor only.
    partial class Mapper
    {
        // A => B mapper.
        private IMapper<A, B> mapper;

        public Mapper()
        {
            // We know A => B mapper is inside same class.
            this.mapper = (IMapper<A, B>)this;
        }
    }
}

```

## Caveats

### Multiple mappers with same source type

Consider example:

```csharp
public class A {}
public class B {}
public class C {}

[MappingGenerator(typeof(A), typeof(B))]
[MappingGenerator(typeof(A), typeof(C))]
public partial class Mapper
{}
```

The code above will cause compiler error because MappingGenerator would have to generate two members with same name and parameters:

```csharp
partial class Mapper : IMapper<A, B>, IMapper<A, C>
{
    public B Map(A source) {}

    // Error. Same name and same parameters.
    public C Map(A source) {}
}
```

To avoid this situation you can inform MappingGenerator to generate explicit `IMapper` interface implementations.

```csharp
[MappingGenerator(typeof(A), typeof(B), ImplementationType = ImplementationType.Explicit)]
[MappingGenerator(typeof(A), typeof(C), ImplementationType = ImplementationType.Explicit)]
public partial class Mapper
{}
```

With the fix code compiles fine:

```csharp
```csharp
partial class Mapper : IMapper<A, B>, IMapper<A, C>
{
    B IMapper<A, B>.Map(A source) {}

    // No conflict.
    C IMapper<A, C>.Map(A source) {}
}
```

### Distinguishing custom methods

Consider example:

```csharp
public class A
{
    public string Text { get; set; }
}

public class B
{
    public string Text { get; set; }
}

public class C
{
    public string Text { get; set; }
}

public class D
{
    public string Text { get; set; }
}

[MappingGenerator(typeof(A), typeof(B))]
[MappingGenerator(typeof(A), typeof(C))]
public partial class Mapper
{
    // Which mapping this applies to? A => B? A => C? Both?
    public string MapText(A source)
    {}
}
```

By default MappingGenerator will use `MapText` for both mappers. If it is not desired behavior you can resolve ambiguity by naming your mappers:

```csharp
[MappingGenerator(typeof(A), typeof(B), Name = "AB")]
[MappingGenerator(typeof(A), typeof(C), Name = "AC")]
public partial class Mapper
{
    // Used only for mapper named AB.
    public string ABMapText(A source)
    {}

    // Used only for mapper named AC.
    public string ACMapText(A source)
    {}
}
```

The same applies for all customization methods like `Map`, `Convert`, `CreateDestination` etc.

### Open (Unbounded) generics

Having multiply open generic mappers in same anchor class generally is not good idea. In this case number of type parameters must be the same across all mappers and match number of type parameters of anchor class:

```csharp
public class A<T> {}
public class B<T> {}
public class C<T> {}
public class D<T> {}

[MappingGenerator(typeof(A<>), typeof(B<>))]
[MappingGenerator(typeof(C<>), typeof(D<>))]
public partial class Mapper<T1, T2>
{}
```
