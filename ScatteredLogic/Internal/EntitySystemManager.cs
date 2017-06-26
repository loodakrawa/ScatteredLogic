// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;
using ScatteredLogic.Internal.DataStructures;
using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class EntitySystemManager<B> : EntityManager<B>, IEntitySystemManager where B : IBitmask<B>
    {
        private readonly SystemManager<B> sm;

        private readonly EntitySet entitiesToRemove;
        private readonly EntitySet dirtyEntities;

        private readonly Queue<Pair<int, int>> componentsToRemove = new Queue<Pair<int, int>>();
        private readonly Queue<ISystem> systemsToRemove = new Queue<ISystem>();
        private readonly Queue<ISystem> systemsToAdd = new Queue<ISystem>();

        private readonly B[] masks;

        public EntitySystemManager(int maxComponents, int maxEntities) : base(maxComponents, maxEntities)
        {
            sm = new SystemManager<B>(maxEntities);

            entitiesToRemove = new EntitySet(maxEntities);
            dirtyEntities = new EntitySet(maxEntities);
            masks = new B[maxEntities];
        }

        public override Entity CreateEntity()
        {
            Entity entity = base.CreateEntity();
            dirtyEntities.Add(entity);
            return entity;
        }

        public override void DestroyEntity(Entity entity)
        {
            ThrowIfStale(entity);
            entitiesToRemove.Add(entity);
            masks[entity.Id] = default(B);
        }

        public override void AddComponent<T>(Entity entity, T component)
        {
            base.AddComponent(entity, component);
            dirtyEntities.Add(entity);
            masks[entity.Id] = masks[entity.Id].Set(TypeIndexer.GetTypeId(typeof(T)));
        }

        public override void AddComponent(Entity entity, object component, Type type)
        {
            base.AddComponent(entity, component, type);
            dirtyEntities.Add(entity);
            masks[entity.Id] = masks[entity.Id].Set(TypeIndexer.GetTypeId(type));
        }

        public override void RemoveComponent(Entity entity, Type compType)
        {
            dirtyEntities.Add(entity);

            int id = entity.Id;
            int type = TypeIndexer.GetTypeId(compType);

            // clear bit
            masks[id] = masks[id].Clear(type);

            // add it fo later removal
            componentsToRemove.Enqueue(new Pair<int, int>(id, type));
        }

        public void AddSystem(ISystem system)
        {
            system.EntityManager = this;
            systemsToAdd.Enqueue(system);
        }

        public void RemoveSystem(ISystem system)
        {
            systemsToRemove.Enqueue(system);
            system.EntityManager = null;
        }

        public void Update()
        {
            // process newly added systems
            while (systemsToAdd.Count > 0)
            {
                ISystem system = systemsToAdd.Dequeue();
                sm.AddSystem(system, CreateSystemBitmask(system), Entities, masks);
            }

            // remove systems
            while (systemsToRemove.Count > 0) sm.RemoveSystem(systemsToRemove.Dequeue());

            // process entities
            while (dirtyEntities.Count > 0 || entitiesToRemove.Count > 0)
            {
                while (dirtyEntities.Count > 0)
                {
                    Entity e = dirtyEntities.Pop();
                    sm.AddEntityToSystems(e, masks[e.Id]);
                }
                while (entitiesToRemove.Count > 0) SyncDestroyEntity(entitiesToRemove.Pop());
            }

            while(componentsToRemove.Count > 0)
            {
                var entry = componentsToRemove.Dequeue();

                int id = entry.Item1;
                int compId = entry.Item2;

                // remove only if bitmask is still cleared
                B mask = masks[id];
                if (!mask.Get(compId)) ComponentManager.RemoveComponent(id, compId);
            }
            componentsToRemove.Clear();
        }

        private void SyncDestroyEntity(Entity entity)
        {
            sm.RemoveEntity(entity);
            ComponentManager.RemoveEntity(entity.Id);
            base.DestroyEntity(entity);
        }

        private B CreateSystemBitmask(ISystem system)
        {
            B bitmask = default(B);
            foreach (Type requiredType in system.RequiredComponents)
            {
                int componentIndex = TypeIndexer.GetTypeId(requiredType);
                bitmask = bitmask.Set(componentIndex);
            }
            return bitmask;
        }

        private struct Pair<T1, T2>
        {
            public readonly T1 Item1;
            public readonly T2 Item2;

            public Pair(T1 item1, T2 item2)
            {
                Item1 = item1;
                Item2 = item2;
            }
        }

    }
}
