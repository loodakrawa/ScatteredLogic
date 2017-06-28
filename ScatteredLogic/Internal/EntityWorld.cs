// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;

namespace ScatteredLogic.Internal
{
    internal sealed class EntityWorld : IEntityWorld
    {
        public IArray<Handle> Entities => null;

        private readonly TypeIndexer typeIndexer;
        private readonly ComponentManager componentManager;
        private readonly EntityManager entityManager;

        public EntityWorld(int maxEntities, int maxComponentTypes)
        {
            typeIndexer = new TypeIndexer(maxComponentTypes);
            entityManager = new EntityManager(maxEntities);
            componentManager = new ComponentManager(maxComponentTypes, maxEntities);
        }

        public Handle CreateEntity()
        {
            return entityManager.CreateEntity();
        }

        public void DestroyEntity(Handle entity)
        {
            ThrowIfStale(entity);
            componentManager.RemoveEntity(entity.Index);
            entityManager.DestroyEntity(entity);
        }

        public bool ContainsEntity(Handle entity)
        {
            return entityManager.ContainsEntity(entity);
        }

        public void AddComponent<T>(Handle entity, T component)
        {
            ThrowIfStale(entity);
            componentManager.AddComponent(entity.Index, component, typeIndexer.GetTypeId(typeof(T)));
        }

        public void AddComponent(Handle entity, object component, Type type)
        {
            ThrowIfStale(entity);
            componentManager.AddComponent(entity.Index, component, typeIndexer.GetTypeId(type), type);
        }

        public void RemoveComponent<T>(Handle entity) => RemoveComponent(entity, typeof(T));
        public void RemoveComponent(Handle entity, Type type)
        {
            ThrowIfStale(entity);
            componentManager.RemoveComponent(entity.Index, typeIndexer.GetTypeId(type));
        }

        public T GetComponent<T>(Handle entity)
        {
            ThrowIfStale(entity);
            return componentManager.GetComponent<T>(entity.Index, typeIndexer.GetTypeId(typeof(T)));
        }

        public object GetComponent(Handle entity, Type type)
        {
            ThrowIfStale(entity);
            return componentManager.GetComponent(entity.Index, typeIndexer.GetTypeId(type));
        }

        public IArray<T> GetComponents<T>() => componentManager.GetAllComponents<T>(typeIndexer.GetTypeId(typeof(T)));

        private void ThrowIfStale(Handle entity)
        {
#if DEBUG
            if (!entityManager.ContainsEntity(entity)) throw new ArgumentException("Entity not managed : " + entity);
#endif
        }
    }
}
