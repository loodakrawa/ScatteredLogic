using ScatteredLogic.Internal.Bitmasks;
using ScatteredLogic.Internal.Managers;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ScatteredLogic.Internal
{
    internal class DeltaQueue<B> where B : IBitmask<B>
    {
        private int proxyEntityCount;

        private readonly List<Handle> entitiesToDestroy = new List<Handle>();
        private readonly List<RemoveComponentData> componentsToRemove = new List<RemoveComponentData>();
        private readonly List<AddComponentData> componentsToAdd = new List<AddComponentData>();
        private readonly Dictionary<Type, IComponentQueue> componentQueues = new Dictionary<Type, IComponentQueue>();

        private readonly int maxEntities;
        private readonly TypeIndexManager typeIndexer;
        private readonly Handle[] newEntities;

        public readonly HashSet<Handle> dirtyEntities = new HashSet<Handle>();
        public readonly B[] entityMasks;

        public DeltaQueue(int maxEntities, TypeIndexManager typeIndexer)
        {
            this.maxEntities = maxEntities;
            this.typeIndexer = typeIndexer;
            newEntities = new Handle[maxEntities];
            entityMasks = new B[maxEntities];
            proxyEntityCount = -1;
        }

        public Handle CreateEntity()
        {
            // create a proxy entity and increase the count of created proxies
            int id = Interlocked.Increment(ref proxyEntityCount);
            return new Handle(id);
        }

        public void DestroyEntity(Handle entity)
        {
            lock (entitiesToDestroy) entitiesToDestroy.Add(entity);
        }

        public void AddComponent<T>(Handle entity, T component)
        {
            lock (componentQueues)
            {
                Type type = typeof(T);

                componentQueues.TryGetValue(type, out IComponentQueue icq);
                ComponentQueue<T> cq;

                if (icq != null) cq = icq as ComponentQueue<T>;
                else
                {
                    cq = new ComponentQueue<T>(maxEntities, typeIndexer.GetIndex(type), ConvertEntity);
                    componentQueues[type] = cq;
                }

                cq.Enqueue(entity, component);
            }
        }

        public void AddComponent(Handle entity, object component, Type type)
        {
            lock (componentsToAdd) componentsToAdd.Add(new AddComponentData(entity, type, component));
        }

        public void RemoveComponent(Handle entity, Type type)
        {
            lock(componentsToRemove) componentsToRemove.Add(new RemoveComponentData(entity, type));
        }

        public void Flush(IEntityWorld entityWorld)
        {
            dirtyEntities.Clear();

            // create new entities and add them to appropriate indices
            for (int i = 0; i < proxyEntityCount + 1; ++i) newEntities[i] = entityWorld.CreateEntity();

            foreach (RemoveComponentData rcd in componentsToRemove)
            {
                Handle realEntity = ConvertEntity(rcd.Entity);
                entityWorld.RemoveComponent(realEntity, rcd.Type);
                dirtyEntities.Add(realEntity);
                int index = realEntity.Index;
                entityMasks[index] = entityMasks[index].Clear(typeIndexer.GetIndex(rcd.Type));
            }

            foreach (AddComponentData acd in componentsToAdd)
            {
                Handle realEntity = ConvertEntity(acd.Entity);
                entityWorld.AddComponent(realEntity, acd.Component, acd.Type);
                dirtyEntities.Add(realEntity);
                int index = realEntity.Index;
                entityMasks[index] = entityMasks[index].Set(typeIndexer.GetIndex(acd.Type));
            }

            foreach (IComponentQueue cq in componentQueues.Values) cq.Flush(entityWorld, dirtyEntities, entityMasks);

            foreach (Handle entity in entitiesToDestroy)
            {
                Handle realEntity = ConvertEntity(entity);
                entityWorld.DestroyEntity(realEntity);
                dirtyEntities.Add(realEntity);
                entityMasks[realEntity.Index] = default(B);
            }

            proxyEntityCount = -1;
            entitiesToDestroy.Clear();
            componentsToAdd.Clear();
            componentsToRemove.Clear();
        }

        // converts a proxy entity to a real entity
        private Handle ConvertEntity(Handle entity) => entity.Version != 0 ? entity : newEntities[entity.Index];

        private struct RemoveComponentData
        {
            public readonly Handle Entity;
            public readonly Type Type;

            public RemoveComponentData(Handle entity, Type type)
            {
                Entity = entity;
                Type = type;
            }
        }

        private struct AddComponentData
        {
            public readonly Handle Entity;
            public readonly Type Type;
            public readonly object Component;

            public AddComponentData(Handle entity, Type type, object component)
            {
                Entity = entity;
                Type = type;
                Component = component;
            }
        }

        private class ComponentQueue<T> : IComponentQueue
        {
            public readonly T[] components;
            public readonly Handle[] entities;

            private readonly int componentTypeIndex;
            private readonly Func<Handle, Handle> entityConverter;

            private int count;

            public ComponentQueue(int maxEntities, int componentTypeIndex, Func<Handle, Handle> entityConverter)
            {
                this.componentTypeIndex = componentTypeIndex;
                components = new T[maxEntities];
                entities = new Handle[maxEntities];
                this.entityConverter = entityConverter;
            }

            public void Enqueue(Handle entity, T component)
            {
                entities[count] = entity;
                components[count] = component;
                ++count;
            }

            public void Flush(IEntityWorld entityWorld, HashSet<Handle> dirtyEntities, B[] entityMasks)
            {
                for (int i = 0; i < count; ++i)
                {
                    Handle entity = entities[i];
                    Handle realEntity = entityConverter(entity);
                    entityWorld.AddComponent(realEntity, components[entity.Index]);
                    dirtyEntities.Add(realEntity);
                    int index = realEntity.Index;
                    entityMasks[index] = entityMasks[index].Set(componentTypeIndex);
                }
                count = 0;
            }
        }

        private interface IComponentQueue
        {
            void Flush(IEntityWorld entityWorld, HashSet<Handle> dirtyEntities, B[] entityMasks);
        }
    }
}
