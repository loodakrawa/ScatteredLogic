// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;
using System.Diagnostics;

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class PackedComponentArray
    {
        public int ComponentTypeCount => componentTypeCount;

        /*
                   |  0  |  1  |  2  |  3  |  4  |  5  |  6  |  7  | 
          | Packed |  2  |  7  |  4  |     |     |     |     |     |
          | Sparse |     |     |  0  |     |  2  |     |     |  1  |
        */
        private readonly int[] packedIndices;
        private readonly int[] sparseIndices;
        private int count;


        /*
          typeof(string) index == 2
          typeof(int)    index == 3
          
                       |    0     |    1     |    2     |    3     |    4     |
          | Components |  int[]   | string[] |          |          |          |
          | Packed     |    3     |    2     |          |          |          |
          | Sparse     |          |          |    1     |    0     |          |
        */
        private readonly IArrayWrapper[] components;
        private readonly int[] componentPackedIndices;
        private readonly int[] componentSparseIndices;
        private int componentTypeCount;

        public PackedComponentArray(int maxTypes, int maxElements)
        {
            Debug.Assert(maxTypes > 0);
            Debug.Assert(maxElements > 0);

            sparseIndices = new int[maxElements];
            packedIndices = new int[maxElements];
            for (int i = 0; i < maxElements; ++i) packedIndices[i] = sparseIndices[i] = -1;

            components = new IArrayWrapper[maxTypes];
            componentPackedIndices = new int[maxTypes];
            componentSparseIndices = new int[maxTypes];
        }

        public int GetComponentTypeIndex(int packedIndex)
        {
            Debug.Assert(packedIndex >= 0 && packedIndex < componentTypeCount);

            return componentPackedIndices[packedIndex];
        }

        public void RegisterType(Type type, int typeIndex)
        {
            Debug.Assert(typeIndex >= 0 && typeIndex < componentSparseIndices.Length);
            Debug.Assert(componentTypeCount < componentPackedIndices.Length);

            Type genericType = typeof(ArrayWrapper<>).MakeGenericType(type);
            IArrayWrapper array = Activator.CreateInstance(genericType, packedIndices.Length) as IArrayWrapper;

            components[componentTypeCount] = array;
            componentPackedIndices[componentTypeCount] = typeIndex;
            componentSparseIndices[typeIndex] = componentTypeCount;

            ++componentTypeCount;
        }

        public void AddComponent<T>(int index, T component, int typeIndex)
        {
            Debug.Assert(index >= 0 && index < sparseIndices.Length);
            Debug.Assert(typeIndex >= 0 && typeIndex < componentSparseIndices.Length);

            int packedIndex = sparseIndices[index];

            int packedComponentIndex = componentSparseIndices[typeIndex];
            ArrayWrapper<T> array = components[packedComponentIndex] as ArrayWrapper<T>;
            array[packedIndex] = component;
        }

        public int Add(int index)
        {
            Debug.Assert(index >= 0 && index < sparseIndices.Length);

            int packedIndex = sparseIndices[index];

            // exiting element
            if (packedIndex >= 0) return packedIndex;

            // place it in the next spot in the packed array
            packedIndex = count;

            // update both indices
            packedIndices[packedIndex] = index;
            sparseIndices[index] = packedIndex;

            ++count;

            return packedIndex;
        }

        public void Remove(int index)
        {
            Debug.Assert(index >= 0 && index < sparseIndices.Length);

            int packedIndex = sparseIndices[index];

            int lastPackedIndex = count - 1;
            if (packedIndex != lastPackedIndex)
            {
                int lastSparseIndex = packedIndices[lastPackedIndex];
                packedIndices[packedIndex] = lastSparseIndex;
                packedIndices[lastPackedIndex] = -1;
                sparseIndices[lastSparseIndex] = packedIndex;

                // swap and remove last
                for (int i = 0; i < componentTypeCount; ++i) components[i].SwapAndRemove(packedIndex, lastPackedIndex);
            }
            else
            {
                packedIndices[packedIndex] = -1;
                // remove entity from all component arrays
                for (int i = 0; i < componentTypeCount; ++i) components[i].RemoveElementAt(packedIndex);
            }

            sparseIndices[index] = -1;
            --count;
        }

        public IArray<T> GetArray<T>(int typeIndex) => GetArrayWrapper(typeIndex) as ArrayWrapper<T>;

        public IArrayWrapper GetArrayWrapper(int typeIndex)
        {
            Debug.Assert(typeIndex >= 0 && typeIndex < componentSparseIndices.Length);

            int packedIndex = componentSparseIndices[typeIndex];
            return components[packedIndex];
        }
    }
}
