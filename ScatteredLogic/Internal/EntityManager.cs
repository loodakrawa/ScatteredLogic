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

        protected readonly TypeIndexer TypeIndexer;
        protected readonly ComponentManager ComponentManager;

        private readonly EntitySet entities;
        private readonly Queue<int> freeIndices;

        public EntityManager(int maxComponents, int maxEntities)
        {
            TypeIndexer = new TypeIndexer(maxComponents);
            ComponentManager = new ComponentManager(maxComponents, maxEntities);

            entities = new EntitySet(maxEntities);
            freeIndices = new Queue<int>(maxEntities);

            for (int i = 0; i < maxEntities; ++i) freeIndices.Enqueue(i);
        }

        public virtual Entity CreateEntity()
        {
            // grow if there are no free indices
            if (freeIndices.Count == 0) throw new Exception("Max number of entities reached");

            int index = freeIndices.Dequeue();
            Entity entity = entities[index];

            // if the entity is in the new expanded range, initialise it
            if (index >= entities.Count)
            {
                entity = new Entity(this, index, entity.Version + 1);
                entities.Add(entity);
            }
            return entity;
        }

        public virtual void DestroyEntity(Entity entity)
        {
            ThrowIfStale(entity);

            int id = entity.Id;

            ComponentManager.RemoveEntity(id);
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
            ComponentManager.AddComponent(entity.Id, component, TypeIndexer.GetTypeId(typeof(T)));
        }

        public virtual void AddComponent(Entity entity, object component, Type type)
        {
            ThrowIfStale(entity);
            ComponentManager.AddComponent(entity.Id, component, TypeIndexer.GetTypeId(type), type);
        }

        public void RemoveComponent<T>(Entity entity) => RemoveComponent(entity, typeof(T));
        public virtual void RemoveComponent(Entity entity, Type type)
        {
            ThrowIfStale(entity);
            ComponentManager.RemoveComponent(entity.Id, TypeIndexer.GetTypeId(type));
        }

        public T GetComponent<T>(Entity entity)
        {
            ThrowIfStale(entity);
            return ComponentManager.GetComponent<T>(entity.Id, TypeIndexer.GetTypeId(typeof(T)));
        }

        public object GetComponent(Entity entity, Type type)
        {
            ThrowIfStale(entity);
            return ComponentManager.GetComponent(entity.Id, TypeIndexer.GetTypeId(type));
        }

        public IArray<T> GetComponents<T>() => ComponentManager.GetAllComponents<T>(TypeIndexer.GetTypeId(typeof(T)));

        protected void ThrowIfStale(Entity entity)
        {
#if DEBUG
            if (!ContainsEntity(entity)) throw new ArgumentException("Entity not managed : " + entity);
#endif
        }
    }
}
