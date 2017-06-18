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
        private object[][] components;

        private readonly List<Pair<int, int>> componentsToRemove = new List<Pair<int, int>>();

        private readonly TypeIndexer indexer;

        public ComponentManager(TypeIndexer indexer)
        {
            this.indexer = indexer;
            components = new object[indexer.Max][];
        }

        public void Grow(int count)
        {
            Array.Resize(ref masks, count);
            for (int i = 0; i < indexer.Count; ++i) Array.Resize(ref components[i], count);
        }

        public void AddComponent(int entity, object component, Type type)
        {
            int compId = indexer.GetIndex(type);

            // set component bit
            masks[entity] = masks[entity].Set(compId);

            if (components[compId] == null) components[compId] = new object[masks.Length];

            // add componenet immediately
            var comps = components[compId][entity] = component;
        }

        public void RemoveComponent(int entity, Type type)
        {
            int compId = indexer.GetIndex(type);

            // clear bit
            masks[entity] = masks[entity].Clear(compId);

            // add it fo later removal
            componentsToRemove.Add(Pair.Of(entity, compId));
        }

        public bool HasComponent(int entity, Type type)
        {
            int compId = indexer.GetIndex(type);
            object[] comps = components[compId];
            return comps != null && comps[entity] != null;
        }

        public T GetComponent<T>(int entity, Type type)
        {
            int compId = indexer.GetIndex(type);
            object[] comps = components[compId];
            return comps != null ? (T) components[compId][entity] : default(T);
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
                if(!mask.Get(compId)) components[compId][entity] = null;
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
            for (int i = 0; i < indexer.Count; ++i)
            {
                object[] comps = components[i];
                if (comps != null) comps[entity] = null;
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
