﻿// Copyright (C) The original author or authors
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
        private readonly int maxComponentTypes;

        private readonly PackedHandleArray entities;
        private readonly B[] entityMasks;
        private readonly TypeIndexer typeIndexer;
        private readonly SparseComponentArray componentManager;
        private readonly HandleManager entityManager;

        private readonly Aspect<B>[] aspects;
        private readonly Handle[] aspectHandles;
        private int aspectCount;

        public EntityWorld(int maxEntities, int maxComponentTypes, int maxAspects)
        {
            this.maxEntities = maxEntities;
            this.maxComponentTypes = maxComponentTypes;

            entities = new PackedHandleArray(maxEntities);
            entityMasks = new B[maxEntities];
            typeIndexer = new TypeIndexer(maxComponentTypes);
            entityManager = new HandleManager(maxEntities);
            componentManager = new SparseComponentArray(maxComponentTypes, maxEntities);

            aspects = new Aspect<B>[maxAspects];
            aspectHandles = new Handle[maxAspects];
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
            int index = entity.Index;

            B entityMask = entityMasks[index];
            for (int i = 0; i < aspectCount; ++i)
            {
                Aspect<B> aspect = aspects[i];
                if (entityMask.Contains(aspect.Bitmask)) aspect.Remove(entity);
            }

            entities.Remove(entity);
            componentManager.RemoveAll(index);
            entityManager.Destroy(entity);
            entityMasks[index] = default(B);
        }

        public bool ContainsEntity(Handle entity)
        {
            return entityManager.Contains(entity);
        }

        public void AddComponent<T>(Handle entity, T component)
        {
            ThrowIfStale(entity);
            int entityIndex = entity.Index;
            int typeIndex = typeIndexer.GetIndex(typeof(T));
            componentManager.Add(entity.Index, component, typeIndex);
            entityMasks[entityIndex] = entityMasks[entityIndex].Set(typeIndex);
            UpdateComponentSets(entity);
        }

        public void RemoveComponent<T>(Handle entity)
        {
            ThrowIfStale(entity);
            int entityIndex = entity.Index;
            int typeIndex = typeIndexer.GetIndex(typeof(T));
            componentManager.Remove(entity.Index, typeIndex);
            entityMasks[entityIndex] = entityMasks[entityIndex].Clear(typeIndex);
            UpdateComponentSets(entity);
        }

        public T GetComponent<T>(Handle entity)
        {
            ThrowIfStale(entity);
            return componentManager.Get<T>(entity.Index, typeIndexer.GetIndex(typeof(T)));
        }

        public Handle CreateAspect(IEnumerable<Type> types)
        {
            Aspect<B> aspect = new Aspect<B>(componentManager, maxEntities, maxComponentTypes);
            foreach (Type type in types)
            {
                int typeId = typeIndexer.GetIndex(type);
                aspect.RegisterType(type, typeId);
            }

            Handle handle = new Handle(aspectCount);
            aspects[aspectCount] = aspect;
            aspectHandles[aspectCount] = handle;
            ++aspectCount;
            return handle;
        }

        public IArray<T> GetAspectComponents<T>(Handle handle)
        {
            Aspect<B> set = aspects[handle.Index];
            return set.GetArray<T>(typeIndexer.GetIndex(typeof(T)));
        }

        public IArray<Handle> GetAspectEntities(Handle handle)
        {
            return aspects[handle.Index];
        }

        private void UpdateComponentSets(Handle entity)
        {
            B mask = entityMasks[entity.Index];
            for (int i = 0; i < aspectCount; ++i)
            {
                Aspect<B> set = aspects[i];
                if (mask.Contains(set.Bitmask)) set.Add(entity);
                else if (!mask.Contains(set.Bitmask) && set.Contains(entity)) set.Remove(entity);
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
