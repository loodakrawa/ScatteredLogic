// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System.Diagnostics;

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class PackedArray<T>
    {
        private readonly IntMap map;
        private readonly ArrayWrapper<T> data;

        public PackedArray(int size)
        {
            Debug.Assert(size > 0);

            map = new IntMap(size);
            data = new ArrayWrapper<T>(size);
        }

        public T[] Data => data.Data;
        public int Count => data.Count;

        public bool Contains(int index) => map.Contains(index);

        public int Add(T element, int index)
        {
            int packedIndex = map.Add(index);
            data[packedIndex] = element;
            data.Count = map.Count;
            return packedIndex;
        }

        public void Remove(int index, out int lastPackedIndex, out int packedIndex)
        {
            map.Remove(index, out lastPackedIndex, out packedIndex);

            if (packedIndex != lastPackedIndex) data.Swap(packedIndex, lastPackedIndex);
            data.RemoveElementAt(lastPackedIndex);
            data.Count = map.Count;
        }

        internal void Clear()
        {
            map.Clear();
            data.Clear();
        }
    }
}
