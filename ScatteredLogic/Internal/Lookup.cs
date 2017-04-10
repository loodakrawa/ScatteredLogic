using System;

namespace ScatteredLogic.Internal
{
    internal sealed class Lookup<T>
    {
        private bool[] flags = new bool[0];
        private T[] data = new T[0];

        public T this[int index]
        {
            get
            {
                if (index >= data.Length || !flags[index]) return default(T);
                return data[index];
            }
            set
            {
                Resize(index);
                flags[index] = true;
                data[index] = value;
            }
        }

        public void Clear()
        {
            Array.Clear(flags, 0, data.Length);
            Array.Clear(data, 0, data.Length);
        }

        public bool HasValue(int index)
        {
            return index < data.Length ? flags[index] : false;
        }

        public T Remove(int index)
        {
            if (index >= data.Length) return default(T);
            T value = data[index];
            flags[index] = false;
            data[index] = default(T);
            return value;
        }

        private void Resize(int size)
        {
            if (data.Length > size) return;
            int target = data.Length > 0 ? data.Length : 1;
            while (target <= size) target *= 2;
            Array.Resize(ref flags, target);
            Array.Resize(ref data, target);
        }
    }
}
