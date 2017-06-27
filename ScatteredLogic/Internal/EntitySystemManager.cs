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
    internal sealed class EntitySystemManager<B> : EntityWorld<B>, IEntitySystemManager where B : IBitmask<B>
    {
        private readonly SystemManager<B> sm;

        private readonly HandleSet entitiesToRemove;
        private readonly HandleSet dirtyEntities;

        private readonly Queue<Pair<int, int>> componentsToRemove = new Queue<Pair<int, int>>();
        private readonly Queue<ISystem> systemsToRemove = new Queue<ISystem>();
        private readonly Queue<ISystem> systemsToAdd = new Queue<ISystem>();

        private readonly B[] masks;

        public EntitySystemManager(int maxComponents, int maxEntities) : base(maxComponents, maxEntities)
        {
            sm = new SystemManager<B>(maxEntities);

            entitiesToRemove = new HandleSet(maxEntities);
            dirtyEntities = new HandleSet(maxEntities);
            masks = new B[maxEntities];
        }

        public override Handle CreateEntity()
        {
            Handle entity = base.CreateEntity();
            dirtyEntities.Add(entity);
            return entity;
        }

        public override void DestroyEntity(Handle entity)
        {
            ThrowIfStale(entity);
            entitiesToRemove.Add(entity);
            masks[entity.Index] = default(B);
        }

        public override void AddComponent<T>(Handle entity, T component)
        {
            base.AddComponent(entity, component);
            dirtyEntities.Add(entity);
            masks[entity.Index] = masks[entity.Index].Set(TypeIndexer.GetTypeId(typeof(T)));
        }

        public override void AddComponent(Handle entity, object component, Type type)
        {
            base.AddComponent(entity, component, type);
            dirtyEntities.Add(entity);
            masks[entity.Index] = masks[entity.Index].Set(TypeIndexer.GetTypeId(type));
        }

        public override void RemoveComponent(Handle entity, Type compType)
        {
            dirtyEntities.Add(entity);

            int id = entity.Index;
            int type = TypeIndexer.GetTypeId(compType);

            // clear bit
            masks[id] = masks[id].Clear(type);

            // add it fo later removal
            componentsToRemove.Enqueue(new Pair<int, int>(id, type));
        }

        public void AddSystem(ISystem system)
        {
            system.EntityWorld = this;
            systemsToAdd.Enqueue(system);
        }

        public void RemoveSystem(ISystem system)
        {
            systemsToRemove.Enqueue(system);
            system.EntityWorld = null;
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
                    Handle e = dirtyEntities.Pop();
                    sm.AddEntityToSystems(e, masks[e.Index]);
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

        private void SyncDestroyEntity(Handle entity)
        {
            sm.RemoveEntity(entity);
            ComponentManager.RemoveEntity(entity.Index);
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
