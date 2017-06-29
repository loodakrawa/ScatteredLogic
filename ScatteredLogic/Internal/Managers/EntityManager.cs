using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal.Managers
{
    internal class EntityManager
    {
        private readonly Handle[] entities;
        private readonly Queue<int> freeIndices;

        private readonly int maxEntities;

        public EntityManager(int maxEntities)
        {
            if(maxEntities > Handle.MaxIndex) throw new Exception("Max possible number: " + Handle.MaxIndex);

            this.maxEntities = maxEntities;

            entities = new Handle[maxEntities];
            freeIndices = new Queue<int>(maxEntities);

            for (int i = 0; i < maxEntities; ++i)
            {
                freeIndices.Enqueue(i);
                entities[i] = new Handle(i);
            }
        }

        public virtual Handle Create()
        {
            if (freeIndices.Count == 0) throw new Exception("Max number of entities reached: " + maxEntities);

            int index = freeIndices.Dequeue();
            Handle entity = entities[index].IncrementVersion();
            entities[index] = entity;

            return entity;
        }

        public virtual void Destroy(Handle entity)
        {
            int index = entity.Index;
            freeIndices.Enqueue(index);
        }

        public bool Contains(Handle entity)
        {
            int index = entity.Index;
            return index < maxEntities && entities[index].Version == entity.Version;
        }
    }
}
