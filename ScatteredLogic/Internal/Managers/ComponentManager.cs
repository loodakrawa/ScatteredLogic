﻿// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.DataStructures;
using System;

namespace ScatteredLogic.Internal.Managers
{
    internal sealed class ComponentManager
    {
        private readonly int maxComponents;
        private readonly IArrayWrapper[] componentArrays;

        public ComponentManager(int maxComponentTypes, int maxComponents)
        {
            componentArrays = new IArrayWrapper[maxComponentTypes];
            this.maxComponents = maxComponents;
        }

        public void RemoveAll(int index)
        {
            for (int i = 0; i < componentArrays.Length; ++i) componentArrays[i]?.RemoveElementAt(index);
        }

        public void Add<T>(int index, T component, int typeIndex)
        {
            ArrayWrapper<T> components = componentArrays[typeIndex] as ArrayWrapper<T>;
            if(components == null)
            {
                components = new ArrayWrapper<T>(maxComponents);
                componentArrays[typeIndex] = components;
            }

            components[index] = component;
        }

        public void Add(int id, object component, int typeIndex, Type type)
        {
            IArrayWrapper components = componentArrays[typeIndex];
            if (components == null)
            {
                Type genericType = typeof(ArrayWrapper<>).MakeGenericType(type);
                components = Activator.CreateInstance(genericType, maxComponents) as IArrayWrapper;
                componentArrays[typeIndex] = components;
            }

            components.SetElementAt(component, id);
        }

        public void Remove(int id, int typeIndex)
        {
            componentArrays[typeIndex]?.RemoveElementAt(id);
        }

        public T Get<T>(int id, int typeIndex)
        {
            ArrayWrapper<T> components = componentArrays[typeIndex] as ArrayWrapper<T>;
            return components != null ? components[id] : default(T);
        }

        public object Get(int id, int typeIndex)
        {
            return componentArrays[typeIndex]?.GetElementAt(id);
        }

        public IArray<T> GetAll<T>(int typeIndex)
        {
            ArrayWrapper<T> comps = componentArrays[typeIndex] as ArrayWrapper<T>;
            if (comps == null)
            {
                comps = new ArrayWrapper<T>(maxComponents);
                componentArrays[typeIndex] = comps;
            }
            return comps;
        }
    }
}