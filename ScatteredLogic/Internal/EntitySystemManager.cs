// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmask;
using ScatteredLogic.Internal.Data;
using System;

namespace ScatteredLogic.Internal
{
    internal sealed class EntitySystemManager<B> : EntityManager<B>, IEntitySystemManager where B : IBitmask<B>
    {
        public EventBus EventBus => eventBus;

        private SyncComponentManager<B> cm;
        private readonly SystemManager<B> sm;

        private readonly EntitySet entitiesToRemove;
        private readonly EntitySet dirtyEntities;

        private readonly EventBus eventBus = new EventBus();

        public EntitySystemManager(int maxComponents, int maxEntities) : base(maxComponents, maxEntities)
        {
            sm = new SystemManager<B>(cm, Indexer, maxEntities);

            entitiesToRemove = new EntitySet(maxEntities);
            dirtyEntities = new EntitySet(maxEntities);
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
        } 

        public override void AddComponent<T>(Entity entity, T component)
        {
            base.AddComponent<T>(entity, component);
            dirtyEntities.Add(entity);
        }

        public override void AddComponent(Entity entity, object component, Type type)
        {
            base.AddComponent(entity, component, type);
            dirtyEntities.Add(entity);
        }

        public override void RemoveComponent(Entity entity, Type type)
        {
            base.RemoveComponent(entity, type);
            dirtyEntities.Add(entity);
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
            cm.Update();

            eventBus.Update();
        }

        protected override ComponentManager CreateComponentManager(int maxComponents, int maxEntities)
        {
            cm = new SyncComponentManager<B>(maxComponents, maxEntities);
            return cm;
        }

        private void SyncDestroyEntity(Entity entity)
        {
            sm.RemoveEntitySync(entity);
            cm.RemoveEntity(entity.Id);
            base.DestroyEntity(entity);
        }

    }
}
