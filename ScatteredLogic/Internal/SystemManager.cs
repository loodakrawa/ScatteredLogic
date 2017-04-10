// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class SystemManager<E, B> where E : struct, IEquatable<E> where B : IBitmask<B>
    {
        private readonly ComponentManager<E, B> cm;
        private readonly TypeIndexer componentIndexer;

        private readonly HashSet<ISystem<E>> systems = new HashSet<ISystem<E>>();
        private readonly Dictionary<ISystem<E>, B> systemMasks = new Dictionary<ISystem<E>, B>();
        private readonly Dictionary<ISystem<E>, HashSet<E>> systemEntitites = new Dictionary<ISystem<E>, HashSet<E>>();

        private readonly HashSet<ISystem<E>> systemsToRemove = new HashSet<ISystem<E>>();
        private readonly HashSet<ISystem<E>> newSystems = new HashSet<ISystem<E>>();

        public SystemManager(ComponentManager<E, B> cm, TypeIndexer componentIndexer)
        {
            this.cm = cm;
            this.componentIndexer = componentIndexer;
        }

        public void AddSystem(ISystem<E> system)
        {
            if (systems.Contains(system)) return;

            B bm = default(B);
            foreach (Type requiredType in system.RequiredComponents)
            {
                int componentIndex = componentIndexer.GetIndex(requiredType);
                bm = bm.Set(componentIndex);
            }

            systemMasks[system] = bm;
            HashSet<E> se = new HashSet<E>();
            systemEntitites[system] = se;
            system.Entities = new SetEnumerable<E>(se);
            systems.Add(system);

            system.Added();
            newSystems.Add(system);
        }

        public void RemoveSystem(ISystem<E> system)
        {
            if ((!systems.Contains(system) && !newSystems.Contains(system)) || systemsToRemove.Contains(system)) return;

            systemsToRemove.Add(system);
        }

        public void UpdateSystems(HashSet<E> allEntities)
        {
            foreach (ISystem<E> system in newSystems)
            {
                foreach (E entity in allEntities) AddEntityToSystem(entity, system);
            }
            newSystems.Clear();

            foreach (ISystem<E> system in systemsToRemove) InternalRemoveSystem(system);
            systemsToRemove.Clear();
        }

        public void AddEntityToSystems(E entity)
        {
            foreach (ISystem<E> system in systems) AddEntityToSystem(entity, system);
        }

        private void AddEntityToSystem(E entity, ISystem<E> system)
        {
            B entityMask = cm.GetBitmask(entity);
            B systemMask = systemMasks[system];

            HashSet<E> entities = systemEntitites[system];

            if (entityMask.Contains(systemMask)) if (entities.Add(entity)) system.EntityAdded(entity);
            if (!entityMask.Contains(systemMask)) if (entities.Remove(entity)) system.EntityRemoved(entity);
        }

        public void RemoveEntitySync(E entity)
        {
            foreach (ISystem<E> system in systems) if (systemEntitites[system].Remove(entity)) system.EntityRemoved(entity);
        }

        private void InternalRemoveSystem(ISystem<E> system)
        {
            system.Removed();
            systemMasks.Remove(system);
            systemEntitites.Remove(system);
            systems.Remove(system);
        }
    }
}
