// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class ArrayWrapper<T> : IArrayWrapper, IArray<T>
    {
        public int Count => data.Length;

        private readonly T[] data;

        public ArrayWrapper(int size) => data = new T[size];

        public T this[int i]
        {
            get => data[i];
            set => data[i] = value;
        }

        public void RemoveElementAt(int index) => data[index] = default(T);

        public ArrayEnumerator<T> GetEnumerator() => new ArrayEnumerator<T>(data, data.Length);
    }

    internal interface IArrayWrapper
    {
        void RemoveElementAt(int index);
    }
}
