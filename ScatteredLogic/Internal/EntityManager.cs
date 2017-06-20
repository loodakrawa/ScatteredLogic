// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

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
        private readonly NamingManager nm;

        private readonly Stack<Entity> entitiesToRemove = new Stack<Entity>();
        private readonly Stack<Entity> dirtyEntities = new Stack<Entity>();

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
            nm = new NamingManager();

            this.growthSize = growthSize;

            Grow(initialSize);
        }

        public Entity CreateEntity()
        {
            if (freeIndices.Count == 0) Grow(growthSize);
            int index = freeIndices.Dequeue();
            Entity entity = entities[index];

            ++entityCount;

            dirtyEntities.Push(entity);

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
        }

        public void DestroyEntity(Entity entity)
        {
            CheckStale(entity);
            entitiesToRemove.Push(entity);
            --entityCount;
        }

        public bool ContainsEntity(Entity entity)
        {
            int index = entity.Id;
            return index < entities.Length && entities[index].Version == entity.Version;
        }     

        public void AddComponent<T>(Entity entity, T component)
        {
            CheckStale(entity);
            cm.AddComponent(entity.Id, component, indexer.GetTypeId(typeof(T)));
            dirtyEntities.Push(entity);
        }

        public void AddComponent(Entity entity, object component, Type type)
        {
            CheckStale(entity);
            cm.AddComponent(entity.Id, component, indexer.GetTypeId(type), type);
            dirtyEntities.Push(entity);
        }

        public void RemoveComponent<T>(Entity entity) => RemoveComponent(entity, indexer.GetTypeId(typeof(T)));
        public void RemoveComponent(Entity entity, object component) => RemoveComponent(entity, indexer.GetTypeId(component.GetType()));
        public void RemoveComponent(Entity entity, Type type) => RemoveComponent(entity, indexer.GetTypeId(type));

        public void RemoveComponent(Entity entity, int typeId)
        {
            CheckStale(entity);
            cm.RemoveComponent(entity.Id, typeId);
            dirtyEntities.Push(entity);
        }

        public T GetComponent<T>(Entity entity) => GetComponent<T>(entity, indexer.GetTypeId(typeof(T)));

        public object GetComponent(Entity entity, Type type)
        {
            CheckStale(entity);
            return cm.GetComponent(entity.Id, indexer.GetTypeId(type));
        }

        public T GetComponent<T>(Entity entity, int typeId)
        {
            CheckStale(entity);
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

        private void CheckStale(Entity entity)
        {
#if DEBUG
            if (!ContainsEntity(entity)) throw new ArgumentException("Entity not managed : " + entity);
#endif
        }

        private void InternalRemoveEntity(Entity entity)
        {
            sm.RemoveEntitySync(entity);
            nm.RemoveEntitySync(entity);
            cm.RemoveEntitySync(entity.Id);

            int index = entity.Id;
            entities[index] = new Entity(this, index, entity.Version + 1);
            freeIndices.Enqueue(index);

            --entityCount;
        }

    }
}
