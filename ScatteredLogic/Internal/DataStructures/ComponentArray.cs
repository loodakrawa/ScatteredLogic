// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class ComponentArray<T> : IComponentArray, IArray<T>
    {
        public int Count => data.Length;

        private readonly T[] data;

        public ComponentArray(int size)
        {
            data = new T[size];
        }

        public T this[int i]
        {
            get => data[i];
            set => data[i] = value;
        }

        public void RemoveElementAt(int index) => data[index] = default(T);
        public void SetElementAt(object element, int index) => data[index] = (T) element;
        public object GetElementAt(int index) => data[index];

        public ArrayEnumerator<T> GetEnumerator() => new ArrayEnumerator<T>(data, data.Length);
    }

    internal interface IComponentArray
    {
        void RemoveElementAt(int index);
        void SetElementAt(object element, int index);
        object GetElementAt(int index);
    }
}
