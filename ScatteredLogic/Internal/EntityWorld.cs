// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.DataStructures;
using ScatteredLogic.Internal.Managers;
using System;

namespace ScatteredLogic.Internal
{
    internal sealed class EntityWorld : IEntityWorld
    {
        public IArray<Handle> Entities => entities;

        private readonly HandleSet entities;
        private readonly TypeIndexManager typeIndexer;
        private readonly ComponentManager componentManager;
        private readonly EntityManager entityManager;

        public EntityWorld(int maxEntities, int maxComponentTypes)
        {
            entities = new HandleSet(maxEntities);
            typeIndexer = new TypeIndexManager(maxComponentTypes);
            entityManager = new EntityManager(maxEntities);
            componentManager = new ComponentManager(maxComponentTypes, maxEntities);
        }

        public Handle CreateEntity()
        {
            Handle entity = entityManager.Create();
            entities.Add(entity);
            return entity;
        }

        public void DestroyEntity(Handle entity)
        {
            ThrowIfStale(entity);
            entities.Remove(entity);
            componentManager.RemoveAll(entity.Index);
            entityManager.Destroy(entity);
        }

        public bool ContainsEntity(Handle entity)
        {
            return entityManager.Contains(entity);
        }

        public void AddComponent<T>(Handle entity, T component)
        {
            ThrowIfStale(entity);
            componentManager.Add(entity.Index, component, typeIndexer.GetIndex(typeof(T)));
        }

        public void AddComponent(Handle entity, object component, Type type)
        {
            ThrowIfStale(entity);
            componentManager.Add(entity.Index, component, typeIndexer.GetIndex(type), type);
        }

        public void RemoveComponent<T>(Handle entity) => RemoveComponent(entity, typeof(T));
        public void RemoveComponent(Handle entity, Type type)
        {
            ThrowIfStale(entity);
            componentManager.Remove(entity.Index, typeIndexer.GetIndex(type));
        }

        public T GetComponent<T>(Handle entity)
        {
            ThrowIfStale(entity);
            return componentManager.Get<T>(entity.Index, typeIndexer.GetIndex(typeof(T)));
        }

        public object GetComponent(Handle entity, Type type)
        {
            ThrowIfStale(entity);
            return componentManager.Get(entity.Index, typeIndexer.GetIndex(type));
        }

        public IArray<T> GetComponents<T>() => componentManager.GetAll<T>(typeIndexer.GetIndex(typeof(T)));

        private void ThrowIfStale(Handle entity)
        {
            if (!entityManager.Contains(entity)) throw new ArgumentException("Entity not managed : " + entity);
        }
    }
}
