﻿// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class HandleSet : IArray<Handle>
    {
        public int Count => count;
       
        private readonly Handle[] entities;
        private readonly int[] indices;

        private int count;

        public HandleSet(int size)
        {
            entities = new Handle[size];
            indices = new int[size];

            for (int i = 0; i < size; ++i) indices[i] = -1;
        }

        public Handle this[int i] { get => entities[i]; }

        public void Clear()
        {
            count = 0;
            for (int i = 0; i < indices.Length; ++i) indices[i] = -1;
        }

        public Handle GetWithIndex(int index) => this[indices[index]];

        public void Add(Handle entity)
        {
            int id = entity.Index;
            int existingIndex = indices.Length > id ? indices[id] : -1;

            if (existingIndex >= 0)
            {
                entities[existingIndex] = entity;
            }
            else
            {
                entities[count] = entity;
                indices[entity.Index] = count;
                ++count;
            }
        }

        public void Remove(Handle entity)
        {
            // find position of entity to remove
            int position = indices[entity.Index];

            // return if it's not contained
            if (position < 0) return;

            // remove the index
            indices[entity.Index] = -1;

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

        public Handle Pop()
        {
            Handle last = entities[count - 1];
            Remove(last);
            return last;
        }

        public Handle Get(int entityId)
        {
            return entities[indices[entityId]];
        }

        public ArrayEnumerator<Handle> GetEnumerator() => new ArrayEnumerator<Handle>(entities, count);

        public static void Copy(HandleSet source, HandleSet destination)
        {
            int count = source.count;
            Array.Copy(source.entities, destination.entities, count);
            Array.Copy(source.indices, destination.indices, count);
            destination.count = count;
        }
    }
}
