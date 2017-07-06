// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System.Diagnostics;

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class HandleManager
    {
        private readonly Handle[] handles;
        private readonly ArrayQueue<int> freeIndices;

        private readonly int maxHandles;

        public HandleManager(int maxHandles)
        {
            Debug.Assert(maxHandles > 0 && maxHandles <= Handle.MaxIndex);

            this.maxHandles = maxHandles;

            handles = new Handle[maxHandles];
            freeIndices = new ArrayQueue<int>(maxHandles);

            for (int i = 0; i < maxHandles; ++i)
            {
                freeIndices.Enqueue(i);
                handles[i] = new Handle(i).IncrementVersion();
            }
        }

        public Handle Create()
        {
            Debug.Assert(freeIndices.Count > 0);

            int index = freeIndices.Dequeue();
            return handles[index];
        }

        public void Destroy(Handle handle)
        {
            Debug.Assert(Contains(handle));

            int index = handle.Index;
            handles[index] = handle.IncrementVersion();
            freeIndices.Enqueue(index);
        }

        public bool Contains(Handle handle)
        {
            int index = handle.Index;
            return index >= 0 && index < maxHandles && handles[index].Version == handle.Version;
        }
    }
}
