// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmask;
using ScatteredLogic.Internal.Data;
using System;

namespace ScatteredLogic.Internal
{
    internal class ComponentManager<B> where B : IBitmask<B>
    {
        private IComponentArray[] components;
        private int entityCount;

        public ComponentManager(int maxComponentCount)
        {
            components = new IComponentArray[maxComponentCount];
        }

        public virtual void RemoveEntity(int entity)
        {
            for (int i = 0; i < components.Length; ++i) components[i]?.RemoveElementAt(entity);
        }

        public virtual void AddComponent<T>(int entity, T component, int type)
        {
            ComponentArray<T> comps = components[type] as ComponentArray<T>;
            if(comps == null)
            {
                comps = new ComponentArray<T>();
                comps.Grow(entityCount);
                components[type] = comps;
            }

            // add componenet immediately
            comps[entity] = component;
        }

        public virtual void AddComponent(int entity, object component, int type, Type compType)
        {
            IComponentArray comps = components[type];
            if (comps == null)
            {
                var listType = typeof(ComponentArray<>).MakeGenericType(compType);
                comps = Activator.CreateInstance(listType) as IComponentArray;
                comps.Grow(entityCount);
                components[type] = comps;
            }

            comps.SetElementAt(component, entity);
        }

        public virtual void RemoveComponent(int entity, int type)
        {
            components[type]?.RemoveElementAt(entity);
        }

        public T GetComponent<T>(int entity, int type)
        {
            ComponentArray<T> comps = components[type] as ComponentArray<T>;
            return comps != null ? comps[entity] : default(T);
        }

        public object GetComponent(int entity, int type)
        {
            return components[type]?.GetElementAt(entity);
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
