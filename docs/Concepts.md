# Concepts

## All generated mappings are DI ready

All generated mappers can be registered in DI container and will work out of box. For more information see [Dependency Injection](./DependencyInjection.md) section.

## All generated mappings are reusable

If MappingGenerator generated mapping A => B it will reuse this mapping whenever A => B mapping is required. For more information see [Nested Mappings](./NestedMappings.md) section.

## Mappings for collection types generated out of box

If MappingGenerator is generating mapping A => B it will also generate mappings for collection types of A => B. For more information see [Lists and Arrays](./ListsAndArrays.md) section.
