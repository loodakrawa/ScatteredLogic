// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmask;
using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class SyncComponentManager<B> : ComponentManager where B : IBitmask<B>
    {
        private readonly List<Pair<int, int>> componentsToRemove = new List<Pair<int, int>>();
        private B[] masks;

        public SyncComponentManager(int maxComponentCount) : base(maxComponentCount)
        {
        }

        public override void RemoveEntity(int id)
        {
            base.RemoveEntity(id);
            masks[id] = default(B);
        }

        public override void AddComponent<T>(int id, T component, int type)
        {
            base.AddComponent(id, component, type);
            masks[id] = masks[id].Set(type);
        }

        public override void AddComponent(int id, object component, int type, Type compType)
        {
            base.AddComponent(id, component, type, compType);
            masks[id] = masks[id].Set(type);
        }

        public override void RemoveComponent(int id, int type)
        {
            // clear bit
            masks[id] = masks[id].Clear(type);

            // add it fo later removal
            componentsToRemove.Add(new Pair<int, int>(id, type));
        }

        public B GetBitmask(int id) => masks[id];

        public void Update()
        {
            foreach (var entry in componentsToRemove)
            {
                int id = entry.Item1;
                int compId = entry.Item2;

                // remove only if bitmask is still cleared
                B mask = masks[id];
                if(!mask.Get(compId)) base.RemoveComponent(id, compId);
            }
            componentsToRemove.Clear();
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
