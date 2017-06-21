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

        protected readonly int GrowthSize;
        protected readonly ComponentManager<B> ComponentManager;

        private readonly EntitySet entities = new EntitySet();
        private readonly Queue<int> freeIndices = new Queue<int>();
        private int entityCount;

        public EntityManager(int maxComponentCount, int initialSize, int growthSize)
        {
            Indexer = new TypeIndexer(maxComponentCount);
            ComponentManager = new ComponentManager<B>(maxComponentCount);

            GrowthSize = growthSize;

            Grow(initialSize);
        }

        public Entity CreateEntity()
        {
            if (freeIndices.Count == 0) Grow(entities.Count + GrowthSize);
            int index = freeIndices.Dequeue();
            Entity entity = entities[index];

            ++entityCount;

            return entity;
        }

        public void DestroyEntity(Entity entity)
        {
            ThrowIfStale(entity);

            int id = entity.Id;

            ComponentManager.RemoveEntity(id);
            entities.Add(new Entity(this, id, entity.Version + 1));
            freeIndices.Enqueue(id);

            --entityCount;
        }

        public bool ContainsEntity(Entity entity)
        {
            int index = entity.Id;
            return index < entities.Count && entities[index].Version == entity.Version;
        }     

        public void AddComponent<T>(Entity entity, T component)
        {
            ThrowIfStale(entity);
            ComponentManager.AddComponent(entity.Id, component, Indexer.GetTypeId(typeof(T)));
        }

        public void AddComponent(Entity entity, object component, Type type)
        {
            ThrowIfStale(entity);
            ComponentManager.AddComponent(entity.Id, component, Indexer.GetTypeId(type), type);
        }

        public void RemoveComponent<T>(Entity entity) => RemoveComponent(entity, typeof(T));
        public void RemoveComponent(Entity entity, Type type)
        {
            ThrowIfStale(entity);
            ComponentManager.RemoveComponent(entity.Id, Indexer.GetTypeId(type));
        }

        public T GetComponent<T>(Entity entity)
        {
            ThrowIfStale(entity);
            return ComponentManager.GetComponent<T>(entity.Id, Indexer.GetTypeId(typeof(T)));
        }

        public object GetComponent(Entity entity, Type type)
        {
            ThrowIfStale(entity);
            return ComponentManager.GetComponent(entity.Id, Indexer.GetTypeId(type));
        }

        public IArray<T> GetComponents<T>() => ComponentManager.GetAllComponents<T>(Indexer.GetTypeId(typeof(T)));

        protected virtual void Grow(int size)
        {
            entities.Grow(size);
            int startIndex = entities.Count;
            for (int i = startIndex; i < size; ++i)
            {
                entities.Add(new Entity(this, i, 1));
                freeIndices.Enqueue(i);
            }
            ComponentManager.Grow(entities.Count);
        }

        protected void ThrowIfStale(Entity entity)
        {
#if DEBUG
            if (!ContainsEntity(entity)) throw new ArgumentException("Entity not managed : " + entity);
#endif
        }
    }
}
