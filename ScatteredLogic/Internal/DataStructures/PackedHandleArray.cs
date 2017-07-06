// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;
using System.Diagnostics;

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class PackedHandleArray : IArray<Handle>
    {
        public int Count => count;
        public Handle this[int i] { get => entities[i]; }
        public ArrayEnumerator<Handle> GetEnumerator() => new ArrayEnumerator<Handle>(entities, count);

        /*
                     |  0  |  1  |  2  |  3  |  4  |  5  |  6  |  7  | 
          | Entities | e2  | e7  | e4  |     |     |     |     |     |
          | Indices  |     |     |  0  |     |  2  |     |     |  1  |
        */
        private readonly Handle[] entities;
        private readonly int[] indices;

        private int count;

        public PackedHandleArray(int size)
        {
            Debug.Assert(size > 0);

            entities = new Handle[size];
            indices = new int[size];

            for (int i = 0; i < indices.Length; ++i) indices[i] = -1;
        }

        public bool Contains(Handle handle)
        {
            int sparseIndex = handle.Index;
            return sparseIndex >= 0 && sparseIndex < indices.Length && indices[handle.Index] != -1;
        }

        public void Add(Handle handle)
        {
            int sparseIndex = handle.Index;

            Debug.Assert(sparseIndex >= 0 && sparseIndex < indices.Length);
            Debug.Assert(indices[sparseIndex] == -1, "Handle already exists: " + handle);

            entities[count] = handle;
            indices[handle.Index] = count;
            ++count;
        }

        public void Remove(Handle handle)
        {
            int sparseIndex = handle.Index;
            Debug.Assert(sparseIndex >= 0 && sparseIndex < indices.Length);

            int packedIndex = indices[sparseIndex];

            int lastPackedIndex = count - 1;
            if (packedIndex != lastPackedIndex)
            {
                Handle lastEntity = entities[lastPackedIndex];

                // swap last with the one to remove
                entities[packedIndex] = lastEntity;
                indices[lastEntity.Index] = packedIndex;
            }

            indices[sparseIndex] = -1;
            --count;
        }

        internal void Clear()
        {
            count = 0;
            for (int i = 0; i < indices.Length; ++i) indices[i] = -1;
        }
    }
}
