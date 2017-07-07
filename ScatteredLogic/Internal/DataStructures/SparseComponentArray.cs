// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System.Diagnostics;

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
            Debug.Assert(maxTypes > 0);
            Debug.Assert(maxElements > 0);

            this.maxElements = maxElements;
            data = new IArrayWrapper[maxTypes];
        }

        public void RemoveAll(int index)
        {
            Debug.Assert(index >= 0 && index < maxElements);

            for (int i = 0; i < data.Length; ++i) data[i]?.RemoveElementAt(index);
        }

        public void Add<T>(int index, T component, int typeIndex)
        {
            Debug.Assert(index >= 0 && index < maxElements);
            Debug.Assert(typeIndex >= 0 && typeIndex < data.Length);

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
            Debug.Assert(index >= 0 && index < maxElements);
            Debug.Assert(typeIndex >= 0 && typeIndex < data.Length);

            data[typeIndex]?.RemoveElementAt(index);
        }

        public IArray<T> GetArray<T>(int typeIndex)
        {
            Debug.Assert(typeIndex >= 0 && typeIndex < data.Length);

            return data[typeIndex] as ArrayWrapper<T>;
        }

        public IArrayWrapper GetArrayWrapper(int typeIndex)
        {
            Debug.Assert(typeIndex >= 0 && typeIndex < data.Length);

            return data[typeIndex];
        }
    }
}
