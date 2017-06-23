// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Data;
using System;

namespace ScatteredLogic.Internal
{
    internal class ComponentManager
    {
        private readonly IComponentArray[] components;
        private int entityCount;

        public ComponentManager(int maxComponents)
        {
            components = new IComponentArray[maxComponents];
        }

        public virtual void RemoveEntity(int id)
        {
            for (int i = 0; i < components.Length; ++i) components[i]?.RemoveElementAt(id);
        }

        public virtual void AddComponent<T>(int id, T component, int type)
        {
            ComponentArray<T> comps = components[type] as ComponentArray<T>;
            if(comps == null)
            {
                comps = new ComponentArray<T>();
                comps.Grow(entityCount);
                components[type] = comps;
            }

            comps[id] = component;
        }

        public virtual void AddComponent(int id, object component, int type, Type compType)
        {
            IComponentArray comps = components[type];
            if (comps == null)
            {
                var listType = typeof(ComponentArray<>).MakeGenericType(compType);
                comps = Activator.CreateInstance(listType) as IComponentArray;
                comps.Grow(entityCount);
                components[type] = comps;
            }

            comps.SetElementAt(component, id);
        }

        public virtual void RemoveComponent(int id, int type)
        {
            components[type]?.RemoveElementAt(id);
        }

        public T GetComponent<T>(int id, int type)
        {
            ComponentArray<T> comps = components[type] as ComponentArray<T>;
            return comps != null ? comps[id] : default(T);
        }

        public object GetComponent(int id, int type)
        {
            return components[type]?.GetElementAt(id);
        }

        public IArray<T> GetAllComponents<T>(int type)
        {
            ComponentArray<T> comps = components[type] as ComponentArray<T>;
            if (comps == null)
            {
                comps = new ComponentArray<T>();
                comps.Grow(entityCount);
                components[type] = comps;
            }
            return comps;
        }

        public virtual void Grow(int entityCount)
        {
            this.entityCount = entityCount;
            for (int i = 0; i < components.Length; ++i) components[i]?.Grow(entityCount);
        }
    }
}
