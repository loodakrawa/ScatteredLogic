// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmask;
using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class SyncComponentManager<B> : ComponentManager<B> where B : IBitmask<B>
    {
        private B[] masks;
        private readonly List<Pair<int, int>> componentsToRemove = new List<Pair<int, int>>();

        public SyncComponentManager(int maxComponentCount) : base(maxComponentCount)
        {
        }

        public override void RemoveEntity(int entity)
        {
            base.RemoveEntity(entity);
            masks[entity] = default(B);
        }

        public override void AddComponent<T>(int entity, T component, int type)
        {
            base.AddComponent<T>(entity, component, type);
            masks[entity] = masks[entity].Set(type);
        }

        public override void AddComponent(int entity, object component, int type, Type compType)
        {
            base.AddComponent(entity, component, type, compType);
            masks[entity] = masks[entity].Set(type);
        }

        public override void RemoveComponent(int entity, int type)
        {
            // clear bit
            masks[entity] = masks[entity].Clear(type);

            // add it fo later removal
            componentsToRemove.Add(new Pair<int, int>(entity, type));
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
                if(!mask.Get(compId)) base.RemoveComponent(entity, compId);
            }
            componentsToRemove.Clear();
        }

        public void ClearMask(int entity)
        {
            masks[entity] = default(B);
        }

        public override void Grow(int entityCount)
        {
            base.Grow(entityCount);
            Array.Resize(ref masks, entityCount);
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
