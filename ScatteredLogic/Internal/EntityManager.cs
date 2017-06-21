// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmask;
using ScatteredLogic.Internal.Data;
using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal class EntityManager<B> : IEntityManager where B : IBitmask<B>
    {
        public IEntitySet Entities => entities;

        protected readonly TypeIndexer Indexer;
        protected readonly int InitialSize;
        protected readonly int GrowthSize;

        private readonly ComponentManager componentManager;

        private readonly EntitySet entities = new EntitySet();
        private readonly Queue<int> freeIndices = new Queue<int>();

        public EntityManager(int maxComponents, int initialSize, int growthSize)
        {
            Indexer = new TypeIndexer(maxComponents);
            componentManager = CreateComponentManager(maxComponents);

            InitialSize = initialSize;
            GrowthSize = growthSize;
        }

        public virtual Entity CreateEntity()
        {
            if (freeIndices.Count == 0)
            {
                if(entities.Count == 0) Grow(InitialSize);
                else Grow(entities.Count + GrowthSize);
            }
            int index = freeIndices.Dequeue();
            Entity entity = entities[index];

            return entity;
        }

        public virtual void DestroyEntity(Entity entity)
        {
            ThrowIfStale(entity);

            int id = entity.Id;

            componentManager.RemoveEntity(id);
            entities.Add(new Entity(this, id, entity.Version + 1));
            freeIndices.Enqueue(id);
        }

        public bool ContainsEntity(Entity entity)
        {
            int index = entity.Id;
            return index < entities.Count && entities[index].Version == entity.Version;
        }     

        public virtual void AddComponent<T>(Entity entity, T component)
        {
            ThrowIfStale(entity);
            componentManager.AddComponent(entity.Id, component, Indexer.GetTypeId(typeof(T)));
        }

        public virtual void AddComponent(Entity entity, object component, Type type)
        {
            ThrowIfStale(entity);
            componentManager.AddComponent(entity.Id, component, Indexer.GetTypeId(type), type);
        }

        public void RemoveComponent<T>(Entity entity) => RemoveComponent(entity, typeof(T));
        public virtual void RemoveComponent(Entity entity, Type type)
        {
            ThrowIfStale(entity);
            componentManager.RemoveComponent(entity.Id, Indexer.GetTypeId(type));
        }

        public T GetComponent<T>(Entity entity)
        {
            ThrowIfStale(entity);
            return componentManager.GetComponent<T>(entity.Id, Indexer.GetTypeId(typeof(T)));
        }

        public object GetComponent(Entity entity, Type type)
        {
            ThrowIfStale(entity);
            return componentManager.GetComponent(entity.Id, Indexer.GetTypeId(type));
        }

        public IArray<T> GetComponents<T>() => componentManager.GetAllComponents<T>(Indexer.GetTypeId(typeof(T)));

        protected virtual void Grow(int size)
        {
            entities.Grow(size);
            int startIndex = entities.Count;
            for (int i = startIndex; i < size; ++i)
            {
                entities.Add(new Entity(this, i, 1));
                freeIndices.Enqueue(i);
            }
            componentManager.Grow(entities.Count);
        }

        protected virtual ComponentManager CreateComponentManager(int maxComponents)
        {
            return new ComponentManager(maxComponents);
        }

        protected void ThrowIfStale(Entity entity)
        {
#if DEBUG
            if (!ContainsEntity(entity)) throw new ArgumentException("Entity not managed : " + entity);
#endif
        }
    }
}
