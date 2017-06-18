// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class ComponentManager<B> where B : IBitmask<B>
    {
        private B[] masks;
        private IVector[] components;
        private int entityCount;
        private readonly List<Pair<int, int>> componentsToRemove = new List<Pair<int, int>>();

        public ComponentManager(int maxComponentCount)
        {
            components = new IVector[maxComponentCount];
        }

        public void Grow(int entityCount)
        {
            this.entityCount = entityCount;
            Array.Resize(ref masks, entityCount);
            for (int i = 0; i < components.Length; ++i) components[i]?.Grow(entityCount);
        }

        public void AddComponent<T>(int entity, T component, int type)
        {
            Vector<T> comps = components[type] as Vector<T>;
            if(comps == null)
            {
                comps = new Vector<T>(entityCount);
                components[type] = comps;
            }

            // set component bit
            masks[entity] = masks[entity].Set(type);

            // add componenet immediately
            comps[entity] = component;
        }

        public void AddComponent(int entity, object component, int type, Type compType)
        {
            IVector comps = components[type];
            if (comps == null)
            {
                var listType = typeof(Vector<>).MakeGenericType(compType);
                comps = Activator.CreateInstance(listType) as IVector;
                comps.Grow(entityCount);
                components[type] = comps;
            }

            // set component bit
            masks[entity] = masks[entity].Set(type);

            // add componenet immediately
            comps.SetElementAt(component, entity);
        }

        public void RemoveComponent(int entity, int type)
        {
            // clear bit
            masks[entity] = masks[entity].Clear(type);

            // add it fo later removal
            componentsToRemove.Add(Pair.Of(entity, type));
        }

        public bool HasComponent(int entity, int type)
        {
            IVector comps = components[type];
            return comps != null && comps.HasElementAt(entity);
        }

        public T GetComponent<T>(int entity, int type)
        {
            Vector<T> comps = components[type] as Vector<T>;
            return comps != null ? comps[entity] : default(T);
        }

        public B GetBitmask(int entity) => masks[entity];

        public void Update()
        {
            foreach (var entry in componentsToRemove)
            {
                int entity = entry.Item1;
                int compId = entry.Item2;

                // remove only if bitmask is not set
                B mask = masks[entity];
                if(!mask.Get(compId)) components[compId].RemoveElementAt(entity);
            }
            componentsToRemove.Clear();
        }

        public void ClearMask(int entity)
        {
            masks[entity] = default(B);
        }

        public void RemoveEntitySync(int entity)
        {
            masks[entity] = default(B);
            for (int i = 0; i < components.Length; ++i)
            {
                IVector comps = components[i];
                if (comps != null) comps.RemoveElementAt(entity);
            }
        }

        private static class Pair
        {
            public static Pair<T1, T2> Of<T1, T2>(T1 item1, T2 item2) => new Pair<T1, T2>(item1, item2);
        }

        private struct Pair<T1, T2>
        {
            public readonly T1 Item1;
            public readonly T2 Item2;

            public Pair(T1 item1, T2 item2)
            {
                Item1 = item1;
                Item2 = item2;
            }
        }
    }
}
