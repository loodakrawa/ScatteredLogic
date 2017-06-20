// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class SystemManager<B> where B : IBitmask<B>
    {
        private readonly ComponentManager<B> cm;
        private readonly TypeIndexer componentIndexer;

        private readonly HashSet<ISystem> systems = new HashSet<ISystem>();
        private readonly Dictionary<ISystem, B> systemMasks = new Dictionary<ISystem, B>();
        private readonly Dictionary<ISystem, EntitySet> systemEntitites = new Dictionary<ISystem, EntitySet>();

        private readonly HashSet<ISystem> systemsToRemove = new HashSet<ISystem>();
        private readonly HashSet<ISystem> newSystems = new HashSet<ISystem>();

        private int entityCount;

        public SystemManager(ComponentManager<B> cm, TypeIndexer componentIndexer)
        {
            this.cm = cm;
            this.componentIndexer = componentIndexer;
        }

        public void Grow(int capacity)
        {
            entityCount = capacity;
            foreach (EntitySet es in systemEntitites.Values) es.Grow(capacity);
        }

        public void AddSystem(ISystem system)
        {
            if (systems.Contains(system)) return;

            B bm = default(B);
            foreach (Type requiredType in system.RequiredComponents)
            {
                int componentIndex = componentIndexer.GetTypeId(requiredType);
                bm = bm.Set(componentIndex);
            }

            systemMasks[system] = bm;
            EntitySet se = new EntitySet(entityCount);
            systemEntitites[system] = se;
            system.Entities = se;
            systems.Add(system);

            system.Added();
            newSystems.Add(system);
        }

        public void RemoveSystem(ISystem system)
        {
            if ((!systems.Contains(system) && !newSystems.Contains(system)) || systemsToRemove.Contains(system)) return;

            systemsToRemove.Add(system);
        }

        public void UpdateSystems(Entity[] allEntities, int entityCount, float deltaTime)
        {
            foreach (ISystem system in newSystems)
            {
                for (int i=0; i<entityCount; ++i) AddEntityToSystem(allEntities[i], system);
            }
            newSystems.Clear();

            foreach (ISystem system in systemsToRemove) InternalRemoveSystem(system);
            systemsToRemove.Clear();

            foreach (ISystem system in systems) system.Update(deltaTime);
        }

        public void AddEntityToSystems(Entity entity)
        {
            foreach (ISystem system in systems) AddEntityToSystem(entity, system);
        }

        private void AddEntityToSystem(Entity entity, ISystem system)
        {
            B entityMask = cm.GetBitmask(entity.Id);
            B systemMask = systemMasks[system];

            EntitySet entities = systemEntitites[system];

            if (entityMask.Contains(systemMask))
            {
                if (!entities.Contains(entity))
                {
                    entities.Add(entity);
                    system.EntityAdded(entity);
                }
            }
            if (!entityMask.Contains(systemMask))
            {
                if (entities.Contains(entity))
                {
                    entities.Remove(entity);
                    system.EntityRemoved(entity);
                }
            }
        }

        public void RemoveEntitySync(Entity entity)
        {
            foreach (ISystem system in systems)
            {
                EntitySet entities = systemEntitites[system];
                if (entities.Contains(entity))
                {
                    entities.Remove(entity);
                    system.EntityRemoved(entity);
                }
            }
        }

        private void InternalRemoveSystem(ISystem system)
        {
            system.Removed();
            systemMasks.Remove(system);
            systemEntitites.Remove(system);
            systems.Remove(system);
        }
    }
}
