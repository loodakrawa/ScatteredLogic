﻿// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmask;
using ScatteredLogic.Internal.Data;
using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class EntityManager<B> : IEntityManager where B : IBitmask<B>
    {
        public EventBus EventBus => eventBus;

        private readonly TypeIndexer indexer;

        private readonly ComponentManager<B> cm;
        private readonly SystemManager<B> sm;

        private readonly EntitySet entitiesToRemove = new EntitySet();
        private readonly EntitySet dirtyEntities = new EntitySet();

        private readonly EventBus eventBus = new EventBus();

        private readonly int growthSize;

        private Entity[] entities;
        private readonly Queue<int> freeIndices = new Queue<int>();
        private int entityCount;

        public EntityManager(int maxComponentCount, int initialSize, int growthSize)
        {
            indexer = new TypeIndexer(maxComponentCount);
            cm = new ComponentManager<B>(maxComponentCount);
            sm = new SystemManager<B>(cm, indexer);

            this.growthSize = growthSize;

            Grow(initialSize);
        }

        public Entity CreateEntity()
        {
            if (freeIndices.Count == 0) Grow(growthSize);
            int index = freeIndices.Dequeue();
            Entity entity = entities[index];

            ++entityCount;

            dirtyEntities.Add(entity);

            return entity;
        }

        private void Grow(int size)
        {
            int startIndex = entities != null ? entities.Length : 0;
            Array.Resize(ref entities, startIndex + size);
            for (int i = startIndex; i < entities.Length; ++i)
            {
                entities[i] = new Entity(this, i, int.MinValue);
                freeIndices.Enqueue(i);
            }
            cm.Grow(entities.Length);
            sm.Grow(entities.Length);

            entitiesToRemove.Grow(entities.Length);
            dirtyEntities.Grow(entities.Length);
        }

        public void DestroyEntity(Entity entity)
        {
            ThrowIfStale(entity);
            entitiesToRemove.Add(entity);
            --entityCount;
        }

        public bool ContainsEntity(Entity entity)
        {
            int index = entity.Id;
            return index < entities.Length && entities[index].Version == entity.Version;
        }     

        public void AddComponent<T>(Entity entity, T component)
        {
            ThrowIfStale(entity);
            cm.AddComponent(entity.Id, component, indexer.GetTypeId(typeof(T)));
            dirtyEntities.Add(entity);
        }

        public void AddComponent(Entity entity, object component, Type type)
        {
            ThrowIfStale(entity);
            cm.AddComponent(entity.Id, component, indexer.GetTypeId(type), type);
            dirtyEntities.Add(entity);
        }

        public void RemoveComponent<T>(Entity entity) => RemoveComponent(entity, indexer.GetTypeId(typeof(T)));
        public void RemoveComponent(Entity entity, object component) => RemoveComponent(entity, indexer.GetTypeId(component.GetType()));
        public void RemoveComponent(Entity entity, Type type) => RemoveComponent(entity, indexer.GetTypeId(type));

        public void RemoveComponent(Entity entity, int typeId)
        {
            ThrowIfStale(entity);
            cm.RemoveComponent(entity.Id, typeId);
            dirtyEntities.Add(entity);
        }

        public T GetComponent<T>(Entity entity) => GetComponent<T>(entity, indexer.GetTypeId(typeof(T)));

        public object GetComponent(Entity entity, Type type)
        {
            ThrowIfStale(entity);
            return cm.GetComponent(entity.Id, indexer.GetTypeId(type));
        }

        public T GetComponent<T>(Entity entity, int typeId)
        {
            ThrowIfStale(entity);
            return cm.GetComponent<T>(entity.Id, typeId);
        }

        public void AddSystem(ISystem system)
        {
            system.EntityManager = this;
            system.EventBus = eventBus;
            sm.AddSystem(system);
        }

        public void RemoveSystem(ISystem system)
        {
            sm.RemoveSystem(system);
            system.EntityManager = null;
        }

        public int GetTypeId<T>() => GetTypeId(typeof(T));

        public int GetTypeId(Type type) => indexer.GetTypeId(type);

        public IArray<T> GetComponents<T>() => GetComponents<T>(indexer.GetTypeId(typeof(T)));

        public IArray<T> GetComponents<T>(int typeId) => cm.GetComponents<T>(typeId);

        public Entity? FindEntity(Func<Entity, bool> predicate)
        {
            foreach (Entity entity in entities) if (predicate(entity)) return entity;
            return null;
        }

        public void FindEntities(Func<Entity, bool> predicate, ICollection<Entity> results)
        {
            foreach (Entity entity in entities) if (predicate(entity)) results.Add(entity);
        }

        public void Update(float deltaTime)
        {
            while (dirtyEntities.Count > 0 || entitiesToRemove.Count > 0)
            {
                while (dirtyEntities.Count > 0) sm.AddEntityToSystems(dirtyEntities.Pop());
                while (entitiesToRemove.Count > 0) InternalRemoveEntity(entitiesToRemove.Pop());
            }

            sm.UpdateSystems(entities, entityCount, deltaTime);
            cm.Update();

            eventBus.Update();
        }

        private void ThrowIfStale(Entity entity)
        {
#if DEBUG
            if (!ContainsEntity(entity)) throw new ArgumentException("Entity not managed : " + entity);
#endif
        }

        private void InternalRemoveEntity(Entity entity)
        {
            sm.RemoveEntitySync(entity);
            cm.RemoveEntitySync(entity.Id);

            int index = entity.Id;
            entities[index] = new Entity(this, index, entity.Version + 1);
            freeIndices.Enqueue(index);

            --entityCount;
        }

    }
}
