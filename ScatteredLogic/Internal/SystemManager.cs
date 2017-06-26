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
    internal sealed class SystemManager<B> where B : IBitmask<B>
    {
        private readonly int maxEntities;

        private readonly List<B> systemBitmasks = new List<B>();
        private readonly List<ISystem> systems = new List<ISystem>();
        private readonly List<EntitySet> systemEntities = new List<EntitySet>();

        public SystemManager(int maxEntities)
        {
            this.maxEntities = maxEntities;
        }

        public void AddSystem(ISystem system, B systemBitmask, IEntitySet allEntities, B[] bitmasks)
        {
            int index = system.Index;

            // do not add same system more than once
            if (systems.Count > index && systems[index] == system) return;

            // assign it the next index
            index = systems.Count;
            system.Index = index;

            // add it to internal lists
            systems.Add(system);
            systemBitmasks.Add(systemBitmask);

            EntitySet entities = new EntitySet(maxEntities);
            systemEntities.Add(entities);
            system.Entities = entities;

            // try to add all entities to the system
            for (int i = 0; i < allEntities.Count; ++i) AddEntityToSystem(allEntities[i], bitmasks[i], system);

            // notify the system it's been added
            system.Added();
        }

        public void RemoveSystem(ISystem system)
        {
            int index = system.Index;
            if (systems.Count <= index || systems[index] == null) return;

            system.Removed();
            system.Entities = null;
            systems.RemoveAt(index);
            systemBitmasks.RemoveAt(index);
            systemEntities.RemoveAt(index);
        }

        public void AddEntityToSystems(Entity entity, B entityMask)
        {
            foreach (ISystem system in systems) AddEntityToSystem(entity, entityMask, system);
        }

        public void RemoveEntity(Entity entity)
        {
            foreach (ISystem system in systems)
            {
                EntitySet entities = systemEntities[system.Index];
                if (entities.Contains(entity))
                {
                    entities.Remove(entity);
                    system.EntityRemoved(entity);
                }
            }
        }

        private void AddEntityToSystem(Entity entity, B entityMask, ISystem system)
        {
            B systemMask = systemBitmasks[system.Index];
            EntitySet entities = systemEntities[system.Index];

            if (entityMask.Contains(systemMask) && !entities.Contains(entity))
            {
                entities.Add(entity);
                system.EntityAdded(entity);
            }
            if (!entityMask.Contains(systemMask) && entities.Contains(entity))
            {
                entities.Remove(entity);
                system.EntityRemoved(entity);
            }
        }
    }
}
