// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;

namespace ScatteredLogic.Internal.Data
{
    internal sealed class ComponentArray<T> : IComponentArray, IArray<T>
    {
        public int Length => data.Length;

        private T[] data;

        public T this[int i]
        {
            get => data[i];
            set => data[i] = value;
        }

        public void RemoveElementAt(int index)
        {
            data[index] = default(T);
        }

        public void Grow(int capacity)
        {
            Array.Resize(ref data, capacity);
        }

        public void SetElementAt(object element, int index) => this[index] = (T) element;
        
        public object GetElementAt(int index) => this[index];
    }

    internal interface IComponentArray
    {
        void Grow(int capacity);
        void RemoveElementAt(int index);
        void SetElementAt(object element, int index);
        object GetElementAt(int index);
    }
}
