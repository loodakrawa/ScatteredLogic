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
    internal sealed class GroupedEntityWorld<B> : IGroupedEntityWorld where B : IBitmask<B>
    {
        private readonly IEntityWorld ew;
        private readonly int maxEntities;
        private readonly TypeIndexer typeIndexer;

        private readonly List<B> groupMasks = new List<B>();

        private B[] entityMasks;
        private readonly List<HandleSet> groupEntities = new List<HandleSet>();

        private B[] entityMasksDelta;
        private readonly List<HandleSet> groupEntitiesDelta = new List<HandleSet>();

        public GroupedEntityWorld(IEntityWorld ew, int maxEntities, int maxComponentCount)
        {
            this.ew = ew;
            this.maxEntities = maxEntities;
            typeIndexer = new TypeIndexer(maxComponentCount);
            entityMasks = new B[maxEntities];
            entityMasksDelta = new B[maxEntities];
        }

        public int GetGroupId(IEnumerable<Type> types)
        {
            B bitmask = CreateBitmask(types);
            int index = groupMasks.IndexOf(bitmask);

            if (index < 0)
            {
                index = groupMasks.Count;
                groupMasks.Add(bitmask);
                groupEntities.Add(new HandleSet(maxEntities));
                groupEntitiesDelta.Add(new HandleSet(maxEntities));
            }

            return index;
        }

        public void Flush()
        {
            Buffer.BlockCopy(entityMasksDelta, 0, entityMasks, 0, entityMasks.Length);
            for (int i = 0; i < groupEntities.Count; ++i) groupEntities[i].CopyFrom(groupEntitiesDelta[i]);
        }

        public IHandleSet GetEntitiesForGroup(int groupId)
        {
            if (groupId < 0 || groupId >= groupEntities.Count) return null;
            return groupEntities[groupId];
        }

        private void UpdateEntityGroups(Handle entity)
        {
            B mask = entityMasksDelta[entity.Index];

            for(int i=0; i<groupMasks.Count; ++i)
            {
                B groupMask = groupMasks[i];
                HandleSet set = groupEntitiesDelta[i];

                if (mask.Contains(groupMask)) set.Add(entity);
                else set.Remove(entity);
            }
        }

        public void DestroyEntity(Handle entity)
        {
            ew.DestroyEntity(entity);
            entityMasksDelta[entity.Index] = default(B);
            UpdateEntityGroups(entity);
        }

        public void AddComponent<T>(Handle entity, T component)
        {
            ew.AddComponent(entity, component);
            int entityIndex = entity.Index;
            entityMasksDelta[entityIndex] = entityMasksDelta[entityIndex].Set(typeIndexer.GetTypeId(typeof(T)));
            UpdateEntityGroups(entity);
        }

        public void AddComponent(Handle entity, object component, Type type)
        {
            ew.AddComponent(entity, component, type);
            int entityIndex = entity.Index;
            entityMasksDelta[entityIndex] = entityMasksDelta[entityIndex].Set(typeIndexer.GetTypeId(type));
            UpdateEntityGroups(entity);
        }

        public void RemoveComponent<T>(Handle entity)
        {
            ew.RemoveComponent<T>(entity);
            int entityIndex = entity.Index;
            entityMasksDelta[entityIndex] = entityMasksDelta[entityIndex].Clear(typeIndexer.GetTypeId(typeof(T)));
            UpdateEntityGroups(entity);
        }

        public void RemoveComponent(Handle entity, Type type)
        {
            ew.RemoveComponent(entity, type);
            int entityIndex = entity.Index;
            entityMasksDelta[entityIndex] = entityMasksDelta[entityIndex].Clear(typeIndexer.GetTypeId(type));
            UpdateEntityGroups(entity);
        }

        public IHandleSet Entities => ew.Entities;
        public bool ContainsEntity(Handle entity) => ew.ContainsEntity(entity);
        public Handle CreateEntity() => ew.CreateEntity();
        public T GetComponent<T>(Handle entity) => ew.GetComponent<T>(entity);
        public object GetComponent(Handle entity, Type type) => ew.GetComponent(entity, type);
        public IArray<T> GetComponents<T>() => ew.GetComponents<T>();

        private B CreateBitmask(IEnumerable<Type> types)
        {
            B bitmask = default(B);
            foreach (Type type in types)
            {
                int componentIndex = typeIndexer.GetTypeId(type);
                bitmask = bitmask.Set(componentIndex);
            }
            return bitmask;
        }
    }
}
