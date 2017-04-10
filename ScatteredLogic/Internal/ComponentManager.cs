// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class ComponentManager<E, B> where B : IBitmask<B>
    {
        private readonly Dictionary<E, B> masks = new Dictionary<E, B>();
        private readonly Dictionary<Type, Dictionary<E, object>> components = new Dictionary<Type, Dictionary<E, object>>();

        private readonly List<Pair<E, Type>> componentsToRemove = new List<Pair<E, Type>>();

        private readonly TypeIndexer indexer;

        public ComponentManager(TypeIndexer indexer)
        {
            this.indexer = indexer;
        }

        public void AddComponent(E entity, object component, Type type)
        {
            int compId = indexer.GetIndex(type);

            // set component bit
            masks[entity] = masks[entity].Set(compId);

            Dictionary<E, object> ce = components.TryGet(type);
            if (ce == null)
            {
                ce = new Dictionary<E, object>();
                components[type] = ce;
            }

            // add componenet immediately
            ce[entity] = component;
        }

        public void RemoveComponent(E entity, Type type)
        {
            // clear bit
            masks[entity] = masks[entity].Clear(indexer.GetIndex(type));

            // add it fo later removal
            componentsToRemove.Add(Pair.Of(entity, type));
        }

        public bool HasComponent(E entity, Type type)
        {
            Dictionary<E, object> ce = components.TryGet(type);
            return ce != null ? ce.ContainsKey(entity) : false;
        }

        public T GetComponent<T>(E entity, Type type)
        {
            Dictionary<E, object> ce = components.TryGet(type);
            if (ce == null) return default(T);
            return (T)ce.TryGet(entity);
        }

        public void AddEntity(E entity)
        {
            masks[entity] = default(B);
        }

        public B GetBitmask(E entity) => masks.TryGet(entity);

        public void Update()
        {
            foreach (var entry in componentsToRemove)
            {
                components[entry.Item2].Remove(entry.Item1);
            }
            componentsToRemove.Clear();
        }

        public void ClearMask(E entity)
        {
            masks[entity] = default(B);
        }

        public void RemoveEntitySync(E entity)
        {
            masks.Remove(entity);
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
