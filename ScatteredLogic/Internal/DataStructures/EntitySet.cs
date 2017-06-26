// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

namespace ScatteredLogic.Internal.DataStructures
{
    internal sealed class EntitySet : IEntitySet
    {
        public int Count => count;

        private readonly  Entity[] entities;
        private readonly int[] indices;

        private int count;

        public EntitySet(int size)
        {
            entities = new Entity[size];
            indices = new int[size];

            for (int i = 0; i < size; ++i) indices[i] = -1;
        }

        public Entity this[int i] { get => entities[i]; }

        public void Clear()
        {
            count = 0;
            for (int i = 0; i < indices.Length; ++i) indices[i] = -1;
        }

        public void Add(Entity entity)
        {
            int id = entity.Id;
            int existingIndex = indices.Length > id ? indices[id] : -1;

            if (existingIndex >= 0)
            {
                entities[existingIndex] = entity;
            }
            else
            {
                entities[count] = entity;
                indices[entity.Id] = count;
                ++count;
            }
        }

        public bool Contains(Entity entity)
        {
            int id = entity.Id;
            return indices.Length > id ? indices[id] >= 0 : false;
        }

        public void Remove(Entity entity)
        {
            // find position of entity to remove
            int position = indices[entity.Id];

            // remove the index
            indices[entity.Id] = -1;

            // find position of last element
            int positionOfLastElement = count - 1;

            // if this is not the last element, move last element into empty spot
            if(position != positionOfLastElement)
            {
                Entity lastEntity = entities[positionOfLastElement];

                // move last element into the position of the removed element
                entities[position] = lastEntity;

                // replace the last element with empty value
                //entities[positionOfLastElement] = new Entity();

                // update position
                indices[lastEntity.Id] = position;
            }

            --count;
        }

        public Entity Pop()
        {
            Entity last = entities[count - 1];
            Remove(last);
            return last;
        }

        public Entity Get(int entityId)
        {
            return entities[indices[entityId]];
        }

        public EntitySetEnumerator GetEnumerator() => new EntitySetEnumerator(entities, count);
    }
}
