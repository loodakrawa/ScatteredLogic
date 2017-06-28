using ScatteredLogic.Internal.Bitmasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ScatteredLogic.Internal
{
    internal class DeltaQueue<B> where B : IBitmask<B>
    {
        private int count;

        private List<Handle> entitiesToDestroy = new List<Handle>();
        private List<RemoveComponentData> componentsToRemove = new List<RemoveComponentData>();
        private List<AddComponentData> componentsToAdd = new List<AddComponentData>();
        private Dictionary<Type, IComponentQueue> componentQueues = new Dictionary<Type, IComponentQueue>();

        private readonly int maxEntities;
        private readonly TypeIndexer typeIndexer;
        private readonly Handle[] newEntities;

        public readonly HashSet<Handle> dirtyEntities = new HashSet<Handle>();
        public readonly B[] entityMasks;

        public DeltaQueue(int maxEntities, TypeIndexer typeIndexer)
        {
            this.maxEntities = maxEntities;
            this.typeIndexer = typeIndexer;
            newEntities = new Handle[maxEntities];
            entityMasks = new B[maxEntities];
            count = -1;
        }

        public Handle CreateEntity()
        {
            int id = Interlocked.Increment(ref count);
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

                IComponentQueue icq;
                componentQueues.TryGetValue(type, out icq);

                ComponentQueue<T> cq;

                if (icq != null) cq = icq as ComponentQueue<T>;
                else
                {
                    cq = new ComponentQueue<T>(maxEntities, ConvertEntity);
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

            System.Diagnostics.Debug.WriteLine("Flush Creating Entities: " + (count + 1));

            // create new entities and add them to appropriate indices
            for (int i = 0; i < count + 1; ++i) newEntities[i] = entityWorld.CreateEntity();

            foreach (RemoveComponentData rcd in componentsToRemove)
            {
                Handle realEntity = ConvertEntity(rcd.Entity);
                entityWorld.RemoveComponent(realEntity, rcd.Type);
                dirtyEntities.Add(realEntity);
                int index = realEntity.Index;
                entityMasks[index] = entityMasks[index].Clear(typeIndexer.GetTypeId(rcd.Type));
            }

            foreach (AddComponentData acd in componentsToAdd)
            {
                Handle realEntity = ConvertEntity(acd.Entity);
                entityWorld.AddComponent(realEntity, acd.Component, acd.Type);
                dirtyEntities.Add(realEntity);
                int index = realEntity.Index;
                entityMasks[index] = entityMasks[index].Set(typeIndexer.GetTypeId(acd.Type));
            }

            foreach (IComponentQueue cq in componentQueues.Values) cq.Flush(entityWorld, typeIndexer, dirtyEntities, entityMasks);

            System.Diagnostics.Debug.WriteLine("Flush Destroying Entities: " + entitiesToDestroy.Count);

            foreach (Handle entity in entitiesToDestroy)
            {
                Handle realEntity = ConvertEntity(entity);
                entityWorld.DestroyEntity(realEntity);
                dirtyEntities.Add(realEntity);
                entityMasks[realEntity.Index] = default(B);
            }

            count = -1;
            entitiesToDestroy.Clear();
            componentsToAdd.Clear();
            componentsToRemove.Clear();
        }

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
            private readonly T[] components;
            private readonly Handle[] entities;
            private readonly Func<Handle, Handle> entityConverter;

            private int count;

            public ComponentQueue(int maxEntities, Func<Handle, Handle> entityConverter)
            {
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

            public void Flush(IEntityWorld entityWorld, TypeIndexer typeIndexer, HashSet<Handle> dirtyEntities, B[] entityMasks)
            {
                for (int i = 0; i < count; ++i)
                {
                    Handle entity = entities[i];
                    Handle realEntity = entityConverter(entity);
                    entityWorld.AddComponent(realEntity, components[entity.Index]);
                    dirtyEntities.Add(realEntity);
                    int index = realEntity.Index;
                    entityMasks[index] = entityMasks[index].Set(typeIndexer.GetTypeId(typeof(T)));
                }
                count = 0;
            }
        }

        private interface IComponentQueue
        {
            void Flush(IEntityWorld entityWorld, TypeIndexer typeIndexer, HashSet<Handle> dirtyEntities, B[] entityMasks);
        }
    }
}
