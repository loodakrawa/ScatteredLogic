// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System.Diagnostics;

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class ArrayQueue<T>
    {
        public int Count { get; private set; }

        private T[] data;
        private int front;
        private int rear;

        public ArrayQueue(int size)
        {
            Debug.Assert(size > 0);

            data = new T[size];
        }

        public void Enqueue(T element)
        {
            Debug.Assert(Count < data.Length);

            data[rear] = element;
            ++Count;
            ++rear;
            if (rear == data.Length) rear = 0;
        }

        public T Dequeue()
        {
            Debug.Assert(Count > 0);

            T element = data[front];
            data[front] = default(T);

            --Count;
            ++front;
            if (front == data.Length) front = 0;

            return element;
        }

    }
}
