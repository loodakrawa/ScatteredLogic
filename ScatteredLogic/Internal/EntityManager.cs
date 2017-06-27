using ScatteredLogic.Internal.DataStructures;
using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal class EntityManager
    {
        public IHandleSet Entities => entities;

        private readonly HandleSet entities;
        private readonly Queue<int> freeIndices;

        private readonly int maxEntities;

        public EntityManager(int maxEntities)
        {
            if(maxEntities > Handle.IndexMask) throw new Exception("Max possible number: " + Handle.IndexMask);

            this.maxEntities = maxEntities;

            entities = new HandleSet(maxEntities);
            freeIndices = new Queue<int>(maxEntities);

            for (int i = 0; i < maxEntities; ++i) freeIndices.Enqueue(i);
        }

        public virtual Handle CreateEntity()
        {
            // blow up if capacity reached
            if (freeIndices.Count == 0) throw new Exception("Max number of entities reached: " + entities.Count);

            int index = freeIndices.Dequeue();
            Handle entity = entities[index];

            // if the entity is in the new expanded range, initialise it
            if (index >= entities.Count)
            {
                entity = new Handle(index | (entity.Version + 1) << Handle.IndexBits);
                entities.Add(entity);
            }
            return entity;
        }

        public virtual void DestroyEntity(Handle entity)
        {
            entities.Remove(entity);
            freeIndices.Enqueue(entity.Index);
        }

        public bool ContainsEntity(Handle entity)
        {
            int index = entity.Index;
            return index < maxEntities && entities[index].Version == entity.Version;
        }
    }
}
