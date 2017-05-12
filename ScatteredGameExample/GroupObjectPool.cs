using System;

namespace ScatteredGameExample
{
    public class GroupObjectPool<T> where T : new()
    {
        private T[] elements = new T[1];
        private int index;
        
        public T Get()
        {
            EnsureCapacity(index);
            T instance = elements[index];
            if (instance == null)
            {
                instance = new T();
                elements[index] = instance;
            }
            ++index;
            return instance;
        }

        public void ReturnAll()
        {
            index = 0;
        }

        private void EnsureCapacity(int size)
        {
            if (elements.Length > size) return;
            int targetSize = elements.Length;
            while (targetSize <= size) targetSize *= 2;

            Array.Resize(ref elements, targetSize);
        }
    }
}
