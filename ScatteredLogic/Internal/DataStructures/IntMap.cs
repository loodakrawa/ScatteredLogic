// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System.Diagnostics;

namespace ScatteredLogic.Internal.DataStructures
{
    public sealed class IntMap
    {

        /*
                   |  0  |  1  |  2  |  3  |  4  |  5  |  6  |  7  | 
          | Packed |  2  |  7  |  4  |     |     |     |     |     |
          | Sparse |     |     |  0  |     |  2  |     |     |  1  |
        */
        private readonly int[] packed;
        private readonly int[] sparse;

        private int count;

        public IntMap(int size)
        {
            Debug.Assert(size > 0);

            packed = new int[size];
            sparse = new int[size];

            Clear();
        }

        public int Count => count;

        public bool Contains(int sparseIndex)
        {
            Debug.Assert(sparseIndex >= 0);

            return sparseIndex < sparse.Length && sparse[sparseIndex] != -1;
        }

        public int GetPacked(int sparseIndex)
        {
            Debug.Assert(Contains(sparseIndex));

            return sparse[sparseIndex];
        }

        public int GetSparse(int packedIndex)
        {
            Debug.Assert(packedIndex >= 0 && packedIndex < count);

            int sparseIndex = packed[packedIndex];
            Debug.Assert(packedIndex == sparse[sparseIndex]);
            return sparseIndex;
        }

        public int Add(int sparseIndex)
        {
            Debug.Assert(sparseIndex >= 0 && sparseIndex < sparse.Length);

            // Get existing packed index.
            int packedIndex = sparse[sparseIndex];

            // Insert it if not already present.
            if (packedIndex == -1)
            {
                packedIndex = count++;

                sparse[sparseIndex] = packedIndex;
                packed[packedIndex] = sparseIndex;
            }

            return packedIndex;
        }

        public void Clear()
        {
            count = 0;
            for (int i = 0; i < sparse.Length; ++i) sparse[i] = -1;
        }

        public void Remove(int sparseIndex, out int lastPackedIndex, out int packedIndex)
        {
            Debug.Assert(sparseIndex >= 0 && sparseIndex < sparse.Length);

            packedIndex = sparse[sparseIndex];
            Debug.Assert(packedIndex >= 0 && packedIndex < packed.Length);

            // Get last element in packed array.
            lastPackedIndex = --count;

            // If the element to remove is not the last one, swap them.
            if (packedIndex != lastPackedIndex)
            {
                int lastSparseIndex = packed[lastPackedIndex];

                packed[packedIndex] = lastSparseIndex;
                sparse[lastSparseIndex] = packedIndex;
            }

            sparse[sparseIndex] = -1;
        }
    }
}
