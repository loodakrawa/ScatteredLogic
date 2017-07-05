// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class SparseComponentArray
    {
        /*
                     |  0  |  1  |  2  |  3  |
          | Position |  X  |  X  |     |  X  |
          | Velocity |     |  X  |     |     |
          | Texture  |  X  |     |     |     |
        */
        private readonly IArrayWrapper[] data;

        private readonly int maxElements;

        public SparseComponentArray(int maxTypes, int maxElements)
        {
            data = new IArrayWrapper[maxTypes];
            this.maxElements = maxElements;
        }

        public void RemoveAll(int index)
        {
            for (int i = 0; i < data.Length; ++i) data[i]?.RemoveElementAt(index);
        }

        public void Add<T>(int index, T component, int typeIndex)
        {
            ArrayWrapper<T> array = data[typeIndex] as ArrayWrapper<T>;
            if (array == null)
            {
                array = new ArrayWrapper<T>(maxElements);
                data[typeIndex] = array;
            }

            array[index] = component;
        }

        public void Remove(int index, int typeIndex)
        {
            data[typeIndex]?.RemoveElementAt(index);
        }

        public IArray<T> GetArray<T>(int typeIndex) => data[typeIndex] as ArrayWrapper<T>;
        public IArrayWrapper GetArrayWrapper(int typeIndex) => data[typeIndex];
    }
}
