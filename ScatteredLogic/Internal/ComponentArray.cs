using System;

namespace ScatteredLogic.Internal
{
    internal class ComponentArray<T> : IComponentArray, IArray<T>
    {
        public int Length => data.Length;

        private T[] data;
        private bool[] flags;

        public ComponentArray()
        {
        }

        public ComponentArray(int capacity)
        {
            data = new T[capacity];
            flags = new bool[capacity];
        }

        public T this[int i]
        {
            get { return data[i]; }
            set
            {
                data[i] = value;
                flags[i] = true;
            }
        }

        public void RemoveElementAt(int index)
        {
            data[index] = default(T);
            flags[index] = false;
        }

        public bool HasElementAt(int index)
        {
            return flags[index];
        }

        public void Grow(int capacity)
        {
            Array.Resize(ref data, capacity);
            Array.Resize(ref flags, capacity);
        }

        public void SetElementAt(object element, int index) => this[index] = (T) element;
        
        public object GetElementAt(int index) => this[index];
    }

    internal interface IComponentArray
    {
        void Grow(int capacity);
        void RemoveElementAt(int index);
        bool HasElementAt(int index);
        void SetElementAt(object element, int index);
        object GetElementAt(int index);
    }
}
