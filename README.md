# Scattered Logic #

A simple, lightweight and fast Entity Component System C# implementation.

## About ##

I wasn't happy with the available ECS implementations for .NET so I decided to roll my own for the games I make.

## Features ##

### No Garbage ###

Mostly due to struct magic, this implementation does not generate any garbage during runtime.
However, it does allocate memory for internal data management. The amount of memory used is proportional to the max number of managed entities.

### Flexible Entity Definition ###

There is a default implementation of the Entity but it is easy for the users to define the Entity the way they want, even as a plain int, long, GUID, etc.

### Variable Bitmask Size ###

In order to minimise the memory footprint, there are 3 different Bitmask sizes: 32, 64 and 128 bits. The mask size is selectable during instantiation of the Entity Manager.

### Integration Considerations ###

The only requirement is for the Entity to be a struct and to implement IEquatable in order to avoid unnecessary memory allocations. Everything else is done via interfaces to make it easier to integrate with existing code and to avoid imposing any restrictions or required inheritances.