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

        private B[] entityMasks;
        private readonly List<HandleSet> groupEntities = new List<HandleSet>();
        private readonly List<HandleSet> groupEntityChanges = new List<HandleSet>();

        private readonly DeltaQueue<B> deltaQueue;

        public GroupedEntityWorld(IEntityWorld ew, int maxEntities, int maxComponentCount)
        {
            this.ew = ew;
            this.maxEntities = maxEntities;
            typeIndexer = new TypeIndexManager(maxComponentCount);
            entityMasks = new B[maxEntities];
            deltaQueue = new DeltaQueue<B>(maxEntities, typeIndexer);
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
                groupEntityChanges.Add(new HandleSet(maxEntities));
            }

            return index;
        }

        public void Commit()
        {
            foreach (HandleSet set in groupEntityChanges) set.Clear();

            deltaQueue.Flush(ew);

            B[] newBitmasks = deltaQueue.entityMasks;
            System.Diagnostics.Debug.WriteLine("Updating dirty entities: "  + deltaQueue.dirtyEntities.Count);
            foreach (Handle entity in deltaQueue.dirtyEntities) UpdateEntityGroups(entity, newBitmasks[entity.Index]);

            Array.Copy(newBitmasks, entityMasks, entityMasks.Length);
        }

        private void UpdateEntityGroups(Handle entity, B newBitmask)
        {
            B mask = entityMasks[entity.Index];
            if (mask.Equals(newBitmask)) return;

            for (int i = 0; i < groupMasks.Count; ++i)
            {
                B groupMask = groupMasks[i];
                HandleSet set = groupEntities[i];

                if (newBitmask.Contains(groupMask))
                {
                    set.Add(entity);
                    groupEntityChanges[i].Add(entity);
                }
                else set.Remove(entity);
            }
        }

        public IArray<Handle> GetEntitiesForGroup(int groupId)
        {
            if (groupId < 0 || groupId >= groupEntities.Count) return null;
            return groupEntities[groupId];
        }

        public IArray<Handle> GetChangesForGroup(int groupId)
        {
            if (groupId < 0 || groupId >= groupEntityChanges.Count) return null;
            return groupEntityChanges[groupId];
        }

        public Handle CreateEntity() => deltaQueue.CreateEntity();
        public void DestroyEntity(Handle entity) => deltaQueue.DestroyEntity(entity);
        public void AddComponent<T>(Handle entity, T component) => deltaQueue.AddComponent<T>(entity, component);
        public void AddComponent(Handle entity, object component, Type type) => deltaQueue.AddComponent(entity, component, type);
        public void RemoveComponent<T>(Handle entity) => deltaQueue.RemoveComponent(entity, typeof(T));
        public void RemoveComponent(Handle entity, Type type) => deltaQueue.RemoveComponent(entity, type);

        public IArray<Handle> Entities => ew.Entities;
        public bool ContainsEntity(Handle entity) => ew.ContainsEntity(entity);
        public T GetComponent<T>(Handle entity) => ew.GetComponent<T>(entity);
        public object GetComponent(Handle entity, Type type) => ew.GetComponent(entity, type);
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
