// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class PackedHandleArray : IArray<Handle>
    {
        public int Count => count;

        /*
                    |  0  |  1  |  2  |  3  |  4  |  5  |  6  |  7  | 
          |Entities | e2  | e7  | e4  |     |     |     |     |     |
          |Indices  |     |     |  0  |     |  2  |     |     |  1  |
        */
        private readonly Handle[] entities;
        private readonly int[] indices;

        private int count;

        public PackedHandleArray(int size)
        {
            entities = new Handle[size];
            indices = new int[size];

            for (int i = 0; i < size; ++i) indices[i] = -1;
        }

        public Handle this[int i] { get => entities[i]; }

        public bool Contains(Handle handle)
        {
            return indices[handle.Index] != -1;
        }

        public void Add(Handle handle)
        {
            int id = handle.Index;
            int existingIndex = indices.Length > id ? indices[id] : -1;

            if (existingIndex >= 0)
            {
                entities[existingIndex] = handle;
            }
            else
            {
                entities[count] = handle;
                indices[handle.Index] = count;
                ++count;
            }
        }

        public void Remove(Handle handle)
        {
            // find position of handle to remove
            int position = indices[handle.Index];

            // return if it's not contained
            if (position < 0) return;

            // remove the index
            indices[handle.Index] = -1;

            // find position of last element
            int positionOfLastElement = count - 1;

            // if this is not the last element, move last element into empty spot
            if(position != positionOfLastElement)
            {
                Handle lastEntity = entities[positionOfLastElement];

                // move last element into the position of the removed element
                entities[position] = lastEntity;

                // replace the last element with empty value
                //entities[positionOfLastElement] = new Entity();

                // update position
                indices[lastEntity.Index] = position;
            }

            --count;
        }

        public ArrayEnumerator<Handle> GetEnumerator() => new ArrayEnumerator<Handle>(entities, count);
    }
}
