// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmask;
using ScatteredLogic.Internal.Data;
using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class EntitySystemManager<B> : EntityManager<B>, IEntitySystemManager where B : IBitmask<B>
    {
        public EventBus EventBus => eventBus;

        private readonly SystemManager<B> sm;

        private readonly EntitySet entitiesToRemove;
        private readonly EntitySet dirtyEntities;

        private readonly EventBus eventBus = new EventBus();

        private readonly List<Pair<int, int>> componentsToRemove = new List<Pair<int, int>>();
        private readonly B[] masks;

        public EntitySystemManager(int maxComponents, int maxEntities) : base(maxComponents, maxEntities)
        {
            sm = new SystemManager<B>(e => masks[e.Id], Indexer, maxEntities);

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
            base.AddComponent<T>(entity, component);
            dirtyEntities.Add(entity);
            masks[entity.Id] = masks[entity.Id].Set(Indexer.GetTypeId(typeof(T)));
        }

        public override void AddComponent(Entity entity, object component, Type type)
        {
            base.AddComponent(entity, component, type);
            dirtyEntities.Add(entity);
        }

        public override void RemoveComponent(Entity entity, Type compType)
        {
            dirtyEntities.Add(entity);

            int id = entity.Id;
            int type = Indexer.GetTypeId(compType);

            // clear bit
            masks[id] = masks[id].Clear(type);

            // add it fo later removal
            componentsToRemove.Add(new Pair<int, int>(id, type));
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

        public void Update(float deltaTime)
        {
            while (dirtyEntities.Count > 0 || entitiesToRemove.Count > 0)
            {
                while (dirtyEntities.Count > 0) sm.AddEntityToSystems(dirtyEntities.Pop());
                while (entitiesToRemove.Count > 0) SyncDestroyEntity(entitiesToRemove.Pop());
            }

            sm.UpdateSystems(Entities, deltaTime);

            foreach (var entry in componentsToRemove)
            {
                int id = entry.Item1;
                int compId = entry.Item2;

                // remove only if bitmask is still cleared
                B mask = masks[id];
                if (!mask.Get(compId)) ComponentManager.RemoveComponent(id, compId);
            }
            componentsToRemove.Clear();

            eventBus.Update();
        }

        private void SyncDestroyEntity(Entity entity)
        {
            sm.RemoveEntitySync(entity);
            ComponentManager.RemoveEntity(entity.Id);
            base.DestroyEntity(entity);
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
