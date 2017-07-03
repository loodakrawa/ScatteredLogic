// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;
using ScatteredLogic.Internal.DataStructures;
using ScatteredLogic.Internal.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScatteredLogic.Internal
{
    internal sealed class EntityWorld<B> : IEntityWorld where B : struct, IBitmask<B>
    {
        public IArray<Handle> Entities => entities;

        private readonly int maxEntities;

        private readonly HandleSet entities;
        private readonly B[] entityMasks;
        private readonly TypeIdManager typeIndexer;
        private readonly ComponentManager componentManager;
        private readonly HandleManager entityManager;

        private readonly HandleManager componentSetManager;
        private readonly ComponentsSet[] componentSets;
        private readonly B[] componentSetMasks;
        private int componentSetCount;

        public EntityWorld(int maxEntities, int maxComponentTypes, int maxComponentSets)
        {
            this.maxEntities = maxEntities;

            entities = new HandleSet(maxEntities);
            entityMasks = new B[maxEntities];
            typeIndexer = new TypeIdManager(maxComponentTypes);
            entityManager = new HandleManager(maxEntities);
            componentManager = new ComponentManager(maxComponentTypes, maxEntities);

            componentSetManager = new HandleManager(maxComponentSets);
            componentSets = new ComponentsSet[maxComponentSets];
            componentSetMasks = new B[maxComponentSets];
        }

        public Handle CreateEntity()
        {
            Handle entity = entityManager.Create();
            entities.Add(entity);
            entityMasks[entity.Index] = default(B);
            return entity;
        }

        public void DestroyEntity(Handle entity)
        {
            ThrowIfStale(entity);
            entities.Remove(entity);
            componentManager.RemoveAll(entity.Index);
            entityManager.Destroy(entity);
            foreach (ComponentsSet set in componentSets) if (set.ContainsEntity(entity)) set.Remove(entity);
        }

        public bool ContainsEntity(Handle entity)
        {
            return entityManager.Contains(entity);
        }

        public void AddComponent<T>(Handle entity, T component)
        {
            ThrowIfStale(entity);
            int entityIndex = entity.Index;
            int typeIndex = typeIndexer.GetId(typeof(T));
            componentManager.Add(entity.Index, component, typeIndex);
            entityMasks[entityIndex] = entityMasks[entityIndex].Set(typeIndex);
            UpdateComponentSets(entity);
        }

        public void RemoveComponent<T>(Handle entity)
        {
            ThrowIfStale(entity);
            int entityIndex = entity.Index;
            int typeIndex = typeIndexer.GetId(typeof(T));
            componentManager.Remove(entity.Index, typeIndex);
            entityMasks[entityIndex] = entityMasks[entityIndex].Clear(typeIndex);
            UpdateComponentSets(entity);
        }

        public T GetComponent<T>(Handle entity)
        {
            ThrowIfStale(entity);
            return componentManager.Get<T>(entity.Index, typeIndexer.GetId(typeof(T)));
        }

        public void UpdateComponent<T>(Handle entity, T component)
        {
            ThrowIfStale(entity);
            int entityIndex = entity.Index;
            int typeIndex = typeIndexer.GetId(typeof(T));
            componentManager.Add(entity.Index, component, typeIndex);
            B mask = entityMasks[entity.Index];
            for (int i = 0; i < componentSetCount; ++i)
            {
                B setMask = componentSetMasks[i];
                if (mask.Contains(setMask)) componentSets[i].UpdateComponent(entity, component, typeIndexer.GetId(typeof(T)));
            }
        }

        public Handle CreateComponentSet(IEnumerable<Type> types)
        {
            ComponentsSet set = new ComponentsSet(componentManager, maxEntities, types.Count());
            B mask = default(B);
            foreach (Type type in types)
            {
                int typeId = typeIndexer.GetId(type);
                set.RegisterType(type, typeId);
                mask = mask.Set(typeId);
            }
            Handle handle = componentSetManager.Create();
            int index = handle.Index;
            componentSets[index] = set;
            componentSetMasks[index] = mask;
            ++componentSetCount;
            return handle;
        }

        public IArray<T> GetSetComponents<T>(Handle handle)
        {
            ComponentsSet set = componentSets[handle.Index];
            return set.Get<T>(typeIndexer.GetId(typeof(T)));
        }

        private void UpdateComponentSets(Handle entity)
        {
            B mask = entityMasks[entity.Index];
            for (int i = 0; i < componentSetCount; ++i)
            {
                B setMask = componentSetMasks[i];
                ComponentsSet set = componentSets[i];
                if (mask.Contains(setMask) && !set.ContainsEntity(entity)) set.Add(entity);
                else if (!mask.Contains(setMask) && set.ContainsEntity(entity)) set.Remove(entity);
            }
        }

        private void ThrowIfStale(Handle entity)
        {
#if DEBUG
            if (!entityManager.Contains(entity)) throw new ArgumentException("Entity not managed : " + entity);
#endif
        }
    }
}
