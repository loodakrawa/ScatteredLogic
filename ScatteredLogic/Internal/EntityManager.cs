// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class EntityManager<E, B> : IEntityManager<E> where E : struct, IEquatable<E> where B : IBitmask<B>
    {
        private readonly ComponentManager<E, B> cm;
        private readonly SystemManager<E, B> sm;
        private readonly NamingManager<E> nm;

        private readonly HashSet<E> entities = new HashSet<E>();

        private readonly Stack<E> entitiesToRemove = new Stack<E>();
        private readonly Stack<E> dirtyEntities = new Stack<E>();

        private readonly IEntityFactory<E> entityFactory;

        public EntityManager(int length, IEntityFactory<E> entityFactory)
        {
            this.entityFactory = entityFactory;
            TypeIndexer indexer = new TypeIndexer(length);
            cm = new ComponentManager<E, B>(indexer);
            sm = new SystemManager<E, B>(cm, indexer);
            nm = new NamingManager<E>();
        }

        public E CreateEntity()
        {
            E entity = entityFactory.Get();
            cm.AddEntity(entity);
            entities.Add(entity);
            dirtyEntities.Push(entity);
            return entity;
        }

        public void DestroyEntity(E entity)
        {
            CheckStale(entity);
            entitiesToRemove.Push(entity);
        }

        public bool ContainsEntity(E entity) => entities.Contains(entity);

        public void AddComponent<T>(E entity, T component) => AddComponent(entity, component, typeof(T));
        public void RemoveComponent<T>(E entity) => RemoveComponent(entity, typeof(T));
        public void RemoveComponent(E entity, object component) => RemoveComponent(entity, component.GetType());
        public bool HasComponent<T>(E entity) => HasComponent(entity, typeof(T));
        public T GetComponent<T>(E entity) => GetComponent<T>(entity, typeof(T));

        public void AddComponent(E entity, object component, Type type)
        {
            CheckStale(entity);
            cm.AddComponent(entity, component, type);
            dirtyEntities.Push(entity);
        }

        public void RemoveComponent(E entity, Type type)
        {
            CheckStale(entity);
            cm.RemoveComponent(entity, type);
            dirtyEntities.Push(entity);
        }

        public bool HasComponent(E entity, Type type)
        {
            CheckStale(entity);
            return cm.HasComponent(entity, type);
        }

        public T GetComponent<T>(E entity, Type type)
        {
            CheckStale(entity);
            return cm.GetComponent<T>(entity, type);
        }

        public string GetName(E entity)
        {
            CheckStale(entity);
            return nm.GetName(entity);
        }

        public void SetName(E entity, string name)
        {
            CheckStale(entity);
            nm.SetName(entity, name);
        }

        public void AddTag(E entity, string tag)
        {
            CheckStale(entity);
            nm.AddTag(entity, tag);
        }

        public void RemoveTag(E entity, string tag)
        {
            CheckStale(entity);
            nm.RemoveTag(entity, tag);
        }

        public bool HasTag(E entity, string tag)
        {
            CheckStale(entity);
            return nm.HasTag(entity, tag);
        }

        public SetEnumerable<E> GetEntitiesWithTag(string tag) => new SetEnumerable<E>(nm.GetEntitiesWithTag(tag));

        public SetEnumerable<string> GetEntityTags(E entity)
        {
            CheckStale(entity);
            return new SetEnumerable<string>(nm.GetEntityTags(entity));
        }

        public void AddSystem(ISystem<E> system)
        {
            system.EntityManager = this;
            sm.AddSystem(system);
        }

        public void RemoveSystem(ISystem<E> system)
        {
            sm.RemoveSystem(system);
            system.EntityManager = null;
        }

        public E? FindEntity(Func<E, bool> predicate)
        {
            foreach (E entity in entities) if (predicate(entity)) return entity;
            return null;
        }

        public void FindEntities(Func<E, bool> predicate, ICollection<E> results)
        {
            foreach (E entity in entities) if (predicate(entity)) results.Add(entity);
        }

        public SetEnumerable<E> GetAllEntities() => new SetEnumerable<E>(entities);

        public void Update()
        {
            while (dirtyEntities.Count > 0 || entitiesToRemove.Count > 0)
            {
                while (dirtyEntities.Count > 0) sm.AddEntityToSystems(dirtyEntities.Pop());
                while (entitiesToRemove.Count > 0) InternalRemoveEntity(entitiesToRemove.Pop());
            }
            sm.UpdateSystems(entities);
            cm.Update();

        }

        private void CheckStale(E entity)
        {
#if DEBUG
            if (!ContainsEntity(entity)) throw new ArgumentException("E not managed : " + entity);
#endif
        }

        private void InternalRemoveEntity(E entity)
        {
            sm.RemoveEntitySync(entity);
            nm.RemoveEntitySync(entity);
            cm.RemoveEntitySync(entity);
            entities.Remove(entity);
            entityFactory.Return(entity);
        }

    }
}
