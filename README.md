# Scattered Logic #

*Still in heavy development and interface changes before getting to version 1.0.*

A simple, lightweight and fast Entity Component System C# implementation.

## About ##

I wasn't happy with the available ECS implementations for .NET so I decided to roll my own for the games I make.

## Features ##

### No Garbage ###

Mostly due to struct magic, this implementation does not generate any garbage during runtime.
However, it does allocate memory for internal data management. The amount of memory used is proportional to the max number of managed entities.

### Variable Bitmask Size ###

In order to minimise the memory footprint, there are 3 different Bitmask sizes: 32, 64 and 128 bits. The mask size is selectable during instantiation of the Entity Manager.

### Integration Considerations ###

Everything is done via interfaces to make it easier to integrate with existing code and to avoid imposing any restrictions or required inheritances.