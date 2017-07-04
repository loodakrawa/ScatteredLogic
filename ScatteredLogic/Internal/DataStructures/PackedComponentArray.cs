// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class PackedComponentArray
    {
        private int count;
        private readonly int[] packedIndices;
        private readonly int[] sparseIndices;

        public int componentTypeCount;
        private readonly IArrayWrapper[] components;
        public readonly int[] componentPackedIndices;
        private readonly int[] componentSparseIndices;

        public PackedComponentArray(int maxTypes, int maxElements)
        {
            sparseIndices = new int[maxElements];
            packedIndices = new int[maxElements];
            for (int i = 0; i < maxElements; ++i) packedIndices[i] = sparseIndices[i] = -1;

            components = new IArrayWrapper[maxTypes];
            componentPackedIndices = new int[maxTypes];
            componentSparseIndices = new int[maxTypes];
        }

        public void RegisterType(Type type, int typeId)
        {
            Type genericType = typeof(ArrayWrapper<>).MakeGenericType(type);
            IArrayWrapper array = Activator.CreateInstance(genericType, packedIndices.Length) as IArrayWrapper;

            components[componentTypeCount] = array;
            componentPackedIndices[componentTypeCount] = typeId;
            componentSparseIndices[typeId] = componentTypeCount;

            ++componentTypeCount;
        }

        public void AddComponent<T>(int index, T component, int typeIndex)
        {
            int packedIndex = sparseIndices[index];

            int packedComponentIndex = componentPackedIndices[typeIndex];
            ArrayWrapper<T> array = components[packedComponentIndex] as ArrayWrapper<T>;
            array[packedIndex] = component;
        }

        public int Add(int index)
        {
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
            int packedIndex = sparseIndices[index];

            // get last index in packed array
            int lastPackedIndex = count - 1;

            // if the element to remove is not the last
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
            int packedIndex = componentSparseIndices[typeIndex];
            return components[packedIndex];
        }
    }
}
