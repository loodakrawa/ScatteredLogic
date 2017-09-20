// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System.Diagnostics;

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class HandleManager
    {
        private readonly Entity[] handles;
        private readonly RingBuffer<int> freeIndices;

        private readonly int maxHandles;

        public HandleManager(int maxHandles)
        {
            Debug.Assert(maxHandles > 0 && maxHandles <= Entity.IndexMask);

            this.maxHandles = maxHandles;

            handles = new Entity[maxHandles];
            freeIndices = new RingBuffer<int>(maxHandles);

            for (int i = 0; i < maxHandles; ++i)
            {
                freeIndices.Enqueue(i);
                handles[i] = IncrementVersion(new Entity(i));
            }
        }

        public Entity Create()
        {
            Debug.Assert(freeIndices.Count > 0);

            int index = freeIndices.Dequeue();
            return handles[index];
        }

        public void Destroy(Entity entity)
        {
            Debug.Assert(Contains(entity));

            int index = entity.Index;
            handles[index] = IncrementVersion(entity);
            freeIndices.Enqueue(index);
        }

        public bool Contains(Entity entity)
        {
            int index = entity.Index;
            return index >= 0 && index < maxHandles && handles[index].Version == entity.Version;
        }

        private static Entity IncrementVersion(Entity entity)
        {
            int version = entity.Version + 1;
            // don't allow version 0
            if (version == 0) ++version;
            return new Entity(entity.Index | version << Entity.IndexBits);
        }
    }
}
