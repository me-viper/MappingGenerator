# Dependency Injection

## ASP.NET Core

```csharp
public class A { }

public class B { }

public class C { }

[MappingGenerator(typeof(A), typeof(B))]
[MappingGenerator(typeof(A), typeof(C), ImplementationType = ImplementationType.Explicit)]
public partial class Mapper
{
}

var services = new ServiceCollection();

// The ASP.NET Core DI container doesn't natively support registering an 
// implementation as multiple services (sometimes called "forwarding"). 
// Instead, you have to manually delegate resolution of the service to a factory function.
services.AddSingleton<Mapper>();

// You need IAbstractMapper registration if you want to use AggregatedMapper (see section bellow).
services.AddSingleton<IAbstractMapper>(p => p.GetRequiredService<Mapper>());

// Register mapper.
services.AddSingleton<IMapper<A, B>>(p => p.GetRequiredService<Mapper>());

// Due to explicit IMapper implementation cast is needed.
services.AddSingleton<IMapper<A, C>>(p => (IMapper<A, C>)p.GetRequiredService<Mapper>());

var provider = services.BuildServiceProvider();
var mapper = provider.GetRequiredService<IMapper>();

var b = provider.GetRequiredService<IMapper<A, B>>().Map(new A());
var c = provider.GetRequiredService<IMapper<A, C>>().Map(new A());
```

## AggregatedMapper

If you want to have all your mappings resolved in single placeyou can use built-in `AggregatedMapper`

```csharp
var services = new ServiceCollection();

// ... Mappers registration ...

services.AddSingleton<IMapper, AggregatedMapper>();

var provider = services.BuildServiceProvider();
var mapper = provider.GetRequiredService<IMapper>();

var b = mapper.Map<A, B>(new A());
var c = mapper.Map<A, C>(new A());

```
