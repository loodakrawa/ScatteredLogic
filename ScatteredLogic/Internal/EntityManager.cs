using ScatteredLogic.Internal.DataStructures;
using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal class EntityManager
    {
        public IArray<Handle> Entities { get; private set; }

        private readonly Handle[] entities;
        private readonly Queue<int> freeIndices;

        private readonly int maxEntities;
        private int count;

        public EntityManager(int maxEntities)
        {
            if(maxEntities > Handle.IndexMask) throw new Exception("Max possible number: " + Handle.IndexMask);

            this.maxEntities = maxEntities;

            entities = new Handle[maxEntities];
            freeIndices = new Queue<int>(maxEntities);

            Entities = new ArrayWrapper<Handle>(entities);

            for (int i = 0; i < maxEntities; ++i) freeIndices.Enqueue(i);
        }

        public virtual Handle CreateEntity()
        {
            // blow up if capacity reached
            if (freeIndices.Count == 0) throw new Exception("Max number of entities reached: " + maxEntities);

            int index = freeIndices.Dequeue();
            Handle entity = entities[index];

            int version = entity.Version + 1;
            // don't allow version 0
            if (version == 0) ++version;
            entity = new Handle(index | version << Handle.IndexBits);
            entities[index] = entity;

            ++count;
            return entity;
        }

        public virtual void DestroyEntity(Handle entity)
        {
            int index = entity.Index;
            freeIndices.Enqueue(index);
            --count;
        }

        public bool ContainsEntity(Handle entity)
        {
            int index = entity.Index;
            return index < maxEntities && entities[index].Version == entity.Version;
        }
    }
}
