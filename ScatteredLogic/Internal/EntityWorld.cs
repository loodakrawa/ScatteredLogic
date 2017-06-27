// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;
using System;

namespace ScatteredLogic.Internal
{
    internal class EntityWorld<B> : IEntityWorld where B : IBitmask<B>
    {
        public IHandleSet Entities => EntityManager.Entities;

        protected readonly TypeIndexer TypeIndexer;

        protected readonly ComponentManager ComponentManager;
        protected readonly EntityManager EntityManager;

        public EntityWorld(int maxComponents, int maxEntities)
        {
            TypeIndexer = new TypeIndexer(maxComponents);
            EntityManager = new EntityManager(maxEntities);
            ComponentManager = new ComponentManager(maxComponents, maxEntities);
        }

        public virtual Handle CreateEntity()
        {
            return EntityManager.CreateEntity();
        }

        public virtual void DestroyEntity(Handle entity)
        {
            ThrowIfStale(entity);
            ComponentManager.RemoveEntity(entity.Index);
            EntityManager.DestroyEntity(entity);
        }

        public bool ContainsEntity(Handle entity)
        {
            return EntityManager.ContainsEntity(entity);
        }

        public virtual void AddComponent<T>(Handle entity, T component)
        {
            ThrowIfStale(entity);
            ComponentManager.AddComponent(entity.Index, component, TypeIndexer.GetTypeId(typeof(T)));
        }

        public virtual void AddComponent(Handle entity, object component, Type type)
        {
            ThrowIfStale(entity);
            ComponentManager.AddComponent(entity.Index, component, TypeIndexer.GetTypeId(type), type);
        }

        public void RemoveComponent<T>(Handle entity) => RemoveComponent(entity, typeof(T));
        public virtual void RemoveComponent(Handle entity, Type type)
        {
            ThrowIfStale(entity);
            ComponentManager.RemoveComponent(entity.Index, TypeIndexer.GetTypeId(type));
        }

        public T GetComponent<T>(Handle entity)
        {
            ThrowIfStale(entity);
            return ComponentManager.GetComponent<T>(entity.Index, TypeIndexer.GetTypeId(typeof(T)));
        }

        public object GetComponent(Handle entity, Type type)
        {
            ThrowIfStale(entity);
            return ComponentManager.GetComponent(entity.Index, TypeIndexer.GetTypeId(type));
        }

        public IArray<T> GetComponents<T>() => ComponentManager.GetAllComponents<T>(TypeIndexer.GetTypeId(typeof(T)));

        protected void ThrowIfStale(Handle entity)
        {
#if DEBUG
            if (!EntityManager.ContainsEntity(entity)) throw new ArgumentException("Entity not managed : " + entity);
#endif
        }
    }
}
