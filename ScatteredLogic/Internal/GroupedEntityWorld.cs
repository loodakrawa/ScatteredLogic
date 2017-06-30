// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;
using ScatteredLogic.Internal.DataStructures;
using ScatteredLogic.Internal.Managers;
using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class GroupedEntityWorld<B> : IGroupedEntityWorld where B : IBitmask<B>
    {
        private readonly IEntityWorld ew;
        private readonly int maxEntities;
        private readonly TypeIndexManager typeIndexer;

        private readonly List<B> groupMasks = new List<B>();
        private readonly B[] entityMasks;
        private readonly List<HandleSet> groupEntities = new List<HandleSet>();

        public GroupedEntityWorld(IEntityWorld ew, int maxEntities, int maxComponentCount)
        {
            this.ew = ew;
            this.maxEntities = maxEntities;
            typeIndexer = new TypeIndexManager(maxComponentCount);
            entityMasks = new B[maxEntities];
        }

        public int GetGroupId(IEnumerable<Type> types)
        {
            B bitmask = CreateBitmask(types);
            int index = groupMasks.IndexOf(bitmask);

            if (index < 0)
            {
                index = groupMasks.Count;
                groupMasks.Add(bitmask);
                HandleSet entities = new HandleSet(maxEntities);
                groupEntities.Add(entities);

                foreach (Handle entity in Entities) if (entityMasks[entity.Index].Contains(bitmask)) entities.Add(entity);
            }

            return index;
        }

        private void UpdateEntityGroups(Handle entity)
        {
            B mask = entityMasks[entity.Index];

            for (int i = 0; i < groupMasks.Count; ++i)
            {
                B groupMask = groupMasks[i];
                HandleSet set = groupEntities[i];

                if (mask.Contains(groupMask)) set.Add(entity);
                else set.Remove(entity);
            }
        }

        public IArray<Handle> GetEntitiesForGroup(int groupId)
        {
            if (groupId < 0 || groupId >= groupEntities.Count) return null;
            return groupEntities[groupId];
        }

        public void DestroyEntity(Handle entity)
        {
            ew.DestroyEntity(entity);
            entityMasks[entity.Index] = default(B);
            UpdateEntityGroups(entity);
        }

        public void AddComponent<T>(Handle entity, T component)
        {
            ew.AddComponent<T>(entity, component);
            int index = entity.Index;
            entityMasks[index] = entityMasks[index].Set(typeIndexer.GetIndex(typeof(T)));
            UpdateEntityGroups(entity);
        }

        public void RemoveComponent<T>(Handle entity)
        {
            ew.RemoveComponent<T>(entity);
            int index = entity.Index;
            entityMasks[index] = entityMasks[index].Clear(typeIndexer.GetIndex(typeof(T)));
            UpdateEntityGroups(entity);
        }

        public Handle CreateEntity() => ew.CreateEntity();
        public IArray<Handle> Entities => ew.Entities;
        public bool ContainsEntity(Handle entity) => ew.ContainsEntity(entity);
        public T GetComponent<T>(Handle entity) => ew.GetComponent<T>(entity);
        public IArray<T> GetComponents<T>() => ew.GetComponents<T>();

        private B CreateBitmask(IEnumerable<Type> types)
        {
            B bitmask = default(B);
            foreach (Type type in types)
            {
                int componentIndex = typeIndexer.GetIndex(type);
                bitmask = bitmask.Set(componentIndex);
            }
            return bitmask;
        }
    }
}
