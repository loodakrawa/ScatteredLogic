using System;

namespace ScatteredLogic.Internal
{
    internal class ComponentArray<T> : IComponentArray, IArray<T>
    {
        public int Length => data.Length;

        private T[] data;

        public ComponentArray()
        {
        }

        public ComponentArray(int capacity)
        {
            data = new T[capacity];
        }

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
