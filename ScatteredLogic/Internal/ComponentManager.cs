﻿// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.DataStructures;
using System;

namespace ScatteredLogic.Internal
{
    internal sealed class ComponentManager
    {
        private readonly int maxEntities;
        private readonly IArrayWrapper[] components;

        public ComponentManager(int maxComponentTypes, int maxEntities)
        {
            components = new IArrayWrapper[maxComponentTypes];
            this.maxEntities = maxEntities;
        }

        public void RemoveEntity(int id)
        {
            for (int i = 0; i < components.Length; ++i) components[i]?.RemoveElementAt(id);
        }

        public void AddComponent<T>(int id, T component, int type)
        {
            ArrayWrapper<T> comps = components[type] as ArrayWrapper<T>;
            if(comps == null)
            {
                comps = new ArrayWrapper<T>(maxEntities);
                components[type] = comps;
            }

            comps[id] = component;
        }

        public void AddComponent(int id, object component, int type, Type compType)
        {
            IArrayWrapper comps = components[type];
            if (comps == null)
            {
                Type genericType = typeof(ArrayWrapper<>).MakeGenericType(compType);
                comps = Activator.CreateInstance(genericType, maxEntities) as IArrayWrapper;
                components[type] = comps;
            }

            comps.SetElementAt(component, id);
        }

        public void RemoveComponent(int id, int type)
        {
            components[type]?.RemoveElementAt(id);
        }

        public T GetComponent<T>(int id, int type)
        {
            ArrayWrapper<T> comps = components[type] as ArrayWrapper<T>;
            return comps != null ? comps[id] : default(T);
        }

        public object GetComponent(int id, int type)
        {
            return components[type]?.GetElementAt(id);
        }

        public IArray<T> GetAllComponents<T>(int type)
        {
            ArrayWrapper<T> comps = components[type] as ArrayWrapper<T>;
            if (comps == null)
            {
                comps = new ArrayWrapper<T>(maxEntities);
                components[type] = comps;
            }
            return comps;
        }
    }
}
