using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class Manager<E, B> : IEntityManager<E> where E : IEquatable<E> where B : IBitmask<B>
    {
        private readonly ComponentManager<E, B> cm;
        private readonly SystemManager<E, B> sm;
        private readonly NamingManager<E> nm;

        private readonly HashSet<E> entities = new HashSet<E>();

        private readonly HashSet<E> entitiesToRemove = new HashSet<E>();
        private readonly HashSet<E> dirtyEntities = new HashSet<E>();

        private readonly HashSet<E> buffer = new HashSet<E>();

        private readonly IEntityFactory<E> entityFactory;

        public Manager(int length, IEntityFactory<E> entityFactory)
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
            dirtyEntities.Add(entity);
            return entity;
        }

        public void DestroyEntity(E entity)
        {
            CheckStale(entity);
            cm.RemoveEntityImmediate(entity);
            entitiesToRemove.Add(entity);
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
            dirtyEntities.Add(entity);
        }

        public void RemoveComponent(E entity, Type type)
        {
            CheckStale(entity);
            cm.RemoveComponent(entity, type);
            dirtyEntities.Add(entity);
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

        public E FindEntity(Func<E, bool> predicate)
        {
            foreach (E entity in entities) if (predicate(entity)) return entity;
            return default(E);
        }

        public void FindEntities(Func<E, bool> predicate, ICollection<E> results)
        {
            foreach (E entity in entities) if (predicate(entity)) results.Add(entity);
        }

        public SetEnumerable<E> GetAllEntities() => new SetEnumerable<E>(entities);

        public void Update()
        {
            sm.UpdateSystems(entities);

            while (dirtyEntities.Count > 0)
            {
                buffer.Clear();
                foreach (E entity in dirtyEntities) buffer.Add(entity);
                dirtyEntities.Clear();

                sm.Update(buffer);
                cm.Update();
            }

            foreach (E entity in entitiesToRemove) InternalRemoveEntity(entity);
            entitiesToRemove.Clear();
        }

        private void CheckStale(E entity)
        {
#if DEBUG
            if (!ContainsEntity(entity)) throw new ArgumentException("E not managed : " + entity);
#endif
        }

        private void InternalRemoveEntity(E entity)
        {
            cm.RemoveEntitySync(entity);
            nm.RemoveEntitySync(entity);
            sm.RemoveEntitySync(entity);
            entities.Remove(entity);
            entityFactory.Return(entity);
        }

    }
}
