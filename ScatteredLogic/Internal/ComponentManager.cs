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
        private readonly Dictionary<Type, Dictionary<int, object>> components = new Dictionary<Type, Dictionary<int, object>>();

        private readonly List<Pair<int, Type>> componentsToRemove = new List<Pair<int, Type>>();

        private readonly TypeIndexer indexer;

        public ComponentManager(TypeIndexer indexer)
        {
            this.indexer = indexer;
        }

        public void Grow(int count)
        {
            Array.Resize(ref masks, count);
        }

        public void AddComponent(int entity, object component, Type type)
        {
            int compId = indexer.GetIndex(type);

            // set component bit
            masks[entity] = masks[entity].Set(compId);

            Dictionary<int, object> ce = components.TryGet(type);
            if (ce == null)
            {
                ce = new Dictionary<int, object>();
                components[type] = ce;
            }

            // add componenet immediately
            ce[entity] = component;
        }

        public void RemoveComponent(int entity, Type type)
        {
            // clear bit
            masks[entity] = masks[entity].Clear(indexer.GetIndex(type));

            // add it fo later removal
            componentsToRemove.Add(Pair.Of(entity, type));
        }

        public bool HasComponent(int entity, Type type)
        {
            Dictionary<int, object> ce = components.TryGet(type);
            return ce != null ? ce.ContainsKey(entity) : false;
        }

        public T GetComponent<T>(int entity, Type type)
        {
            Dictionary<int, object> ce = components.TryGet(type);
            if (ce == null) return default(T);
            return (T)ce.TryGet(entity);
        }

        public B GetBitmask(int entity) => masks[entity];

        public void Update()
        {
            foreach (var entry in componentsToRemove)
            {
                int entity = entry.Item1;
                Type type = entry.Item2;

                // remove only if bitmask is not set
                B mask = masks[entity];
                if(!mask.Get(indexer.GetIndex(type))) components[entry.Item2].Remove(entry.Item1);
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
            foreach (var entry in components) entry.Value.Remove(entity);
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
