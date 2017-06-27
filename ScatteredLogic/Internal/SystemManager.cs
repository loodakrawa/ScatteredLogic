// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;
using ScatteredLogic.Internal.DataStructures;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class SystemManager<B> where B : IBitmask<B>
    {
        private readonly int maxEntities;

        private readonly List<ISystem> systems = new List<ISystem>();
        private readonly List<B> systemBitmasks = new List<B>();
        private readonly List<HandleSet> systemEntities = new List<HandleSet>();

        public SystemManager(int maxEntities)
        {
            this.maxEntities = maxEntities;
        }

        public void AddSystem(ISystem system, B systemBitmask, IHandleSet allEntities, B[] bitmasks)
        {
            int index = system.Info?.Index ?? 0;

            // do not add same system more than once
            if (systems.Count > index && systems[index] == system) return;

            // assign it the next index
            system.Info = new SystemInfo { Index = systems.Count };

            // add it to internal lists
            systems.Add(system);
            systemBitmasks.Add(systemBitmask);

            HandleSet entities = new HandleSet(maxEntities);
            systemEntities.Add(entities);
            system.Entities = entities;

            // try to add all entities to the system
            for (int i = 0; i < allEntities.Count; ++i) AddEntityToSystem(allEntities[i], bitmasks[i], system);

            // notify the system it's been added
            system.Added();
        }

        public void RemoveSystem(ISystem system)
        {
            if (system.Info == null) return;

            int index = system.Info.Index;
            if (systems.Count <= index || systems[index] == null) return;

            system.Removed();
            system.Entities = null;
            system.Info = null;

            systems.RemoveAt(index);
            systemBitmasks.RemoveAt(index);
            systemEntities.RemoveAt(index);
        }

        public void AddEntityToSystems(Handle entity, B entityMask)
        {
            foreach (ISystem system in systems) AddEntityToSystem(entity, entityMask, system);
        }

        public void RemoveEntity(Handle entity)
        {
            foreach (ISystem system in systems)
            {
                HandleSet entities = systemEntities[system.Info.Index];
                if (entities.Contains(entity))
                {
                    entities.Remove(entity);
                    system.EntityRemoved(entity);
                }
            }
        }

        private void AddEntityToSystem(Handle entity, B entityMask, ISystem system)
        {
            int index = system.Info.Index;

            B systemMask = systemBitmasks[system.Info.Index];
            HandleSet entities = systemEntities[system.Info.Index];

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

        private class SystemInfo : ISystemInfo
        {
            public int Index { get; set; }
        }
    }
}
