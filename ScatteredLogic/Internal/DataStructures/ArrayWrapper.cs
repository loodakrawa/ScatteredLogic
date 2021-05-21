// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;
using System.Diagnostics;

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class ArrayWrapper<T> : IArrayWrapper
    {
        private readonly T[] data;

        public ArrayWrapper(int size)
        {
            Debug.Assert(size > 0);

            data = new T[size];
        }

        public T[] Data => data;

        public T this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < data.Length);

                return data[index];
            }
            set
            {
                Debug.Assert(index >= 0 && index < data.Length);

                data[index] = value;
            }
        }

        public void RemoveElementAt(int index)
        {
            Debug.Assert(index >= 0 && index < data.Length);

            data[index] = default(T);
        }

        public void Swap(int firstIndex, int secondIndex)
        {
            Debug.Assert(firstIndex >= 0 && firstIndex < data.Length);
            Debug.Assert(secondIndex >= 0 && secondIndex < data.Length);

            T tmp = data[firstIndex];
            data[firstIndex] = data[secondIndex];
            data[secondIndex] = tmp;
        }

        public void AddFrom(int thisIndex, IArrayWrapper other, int otherIndex)
        {
            Debug.Assert(thisIndex >= 0 && thisIndex < data.Length);
            Debug.Assert(other is ArrayWrapper<T>);

            data[thisIndex] = (other as ArrayWrapper<T>)[otherIndex];
        }

        public void Clear()
        {
            Array.Clear(data, 0, data.Length);
        }
    }

    internal interface IArrayWrapper
    {
        void RemoveElementAt(int index);
        void Swap(int firstIndex, int secondIndex);
        void AddFrom(int index, IArrayWrapper other, int otherIndex);
        void Clear();
    }
}
