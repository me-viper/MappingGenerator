# MappingGenerator

MappingGenerator is C# [source generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) that allows generating object mapping code on compilation stage.

Having source code for your mappings generated provides the following benefits:

* Your mappings are always up to date.
* You see code for all your mappings. Nothing is hidden.
* All debugging features are available. You can step-in to your mappings, set breakpoints etc.
* If mapping can't be done or has issues you get compiler errors rather than runtime errors.

**Note**. C# source generators require NET5.0 or higher.

For more information check out the [guide](https://mappinggenerator.readthedocs.io/en/latest/index.html).

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

For more information check out [Getting Started](https://mappinggenerator.readthedocs.io/en/latest/GettingStarted.html) guide.
