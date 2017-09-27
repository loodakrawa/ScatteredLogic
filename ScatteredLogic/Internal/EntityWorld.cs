// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;
using ScatteredLogic.Internal.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ScatteredLogic.Internal
{
    internal sealed class EntityWorld<B> : IEntityWorld where B : struct, IBitmask<B>
    {
        public int EntityCount => entities.Count;
        public Entity[] Entities => entities.Data;

        private readonly int maxEntities;
        private readonly int maxComponentTypes;

        private readonly PackedArray<Entity> entities;
        private readonly B[] entityMasks;
        private readonly TypeIndexer typeIndexer;
        private readonly SparseComponentArray sparseComponents;
        private readonly HandleManager entityManager;

        private readonly List<Aspect<B>> aspects = new List<Aspect<B>>();

        private readonly PackedArray<Entity> dirtyEntities;
        private readonly ThreadLocal<ChangeQueue> changeQueue;

        private readonly PackedArray<Entity> entitiesToDestroy;

        public EntityWorld(int maxEntities, int maxEvents, int maxComponentTypes)
        {
            this.maxEntities = maxEntities;
            this.maxComponentTypes = maxComponentTypes;

            entities = new PackedArray<Entity>(maxEntities);
            entityMasks = new B[maxEntities];

            typeIndexer = new TypeIndexer(maxComponentTypes);
            entityManager = new HandleManager(maxEntities);
            sparseComponents = new SparseComponentArray(maxComponentTypes, maxEntities);

            changeQueue = new ThreadLocal<ChangeQueue>(() => new ChangeQueue(maxComponentTypes, maxEvents), true);
            dirtyEntities = new PackedArray<Entity>(maxEntities);
            entitiesToDestroy = new PackedArray<Entity>(maxEntities);
        }

        public Entity CreateEntity()
        {
            lock (entityManager)
            {
                Entity entity = entityManager.Create();
                entities.Add(entity, entity.Index);
                entityMasks[entity.Index] = default(B);
                return entity;
            }
        }

        public void DestroyEntity(Entity entity)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);

            lock (entitiesToDestroy) entitiesToDestroy.Add(entity, entity.Index);
        }

        public bool ContainsEntity(Entity entity)
        {
            return entityManager.Contains(entity);
        }

        public void AddComponent<T>(Entity entity, T component)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);

            changeQueue.Value.AddComponent(entity, component, typeIndexer.GetIndex(typeof(T)));
        }

        public void RemoveComponent<T>(Entity entity)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);

            changeQueue.Value.RemoveComponent<T>(entity, typeIndexer.GetIndex(typeof(T)));
        }

        internal void AddComponent<T>(Entity entity, T component, int typeIndex)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);

            int entityIndex = entity.Index;

            sparseComponents.Add(entity.Index, component, typeIndex);
            entityMasks[entityIndex] = entityMasks[entityIndex].Set(typeIndex);

            dirtyEntities.Add(entity, entityIndex);
        }

        internal void RemoveComponent<T>(Entity entity, int typeIndex)
        {
            int entityIndex = entity.Index;

            sparseComponents.Remove(entityIndex, typeIndex);
            entityMasks[entityIndex] = entityMasks[entityIndex].Clear(typeIndex);

            dirtyEntities.Add(entity, entityIndex);
        }

        public T GetComponent<T>(Entity entity)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);

            int typeId = typeIndexer.GetIndex(typeof(T));

            if (changeQueue.Value.GetComponent(entity, typeId, out T component)) return component;

            ArrayWrapper<T> array = sparseComponents.GetArray<T>(typeId);
            return array != null ? array.Data[entity.Index] : default(T);
        }

        public IAspect CreateAspect(Type[] types)
        {
            Aspect<B> aspect = new Aspect<B>(sparseComponents, typeIndexer, maxEntities, maxComponentTypes, types);
            aspects.Add(aspect);
            return aspect;
        }

        public void Commit()
        {
            changeQueue.Value.Flush(this);

            Entity[] dirtyEntityData = dirtyEntities.Data;
            Entity[] entitiesToDestroyData = entitiesToDestroy.Data;

            foreach (Aspect<B> aspect in aspects)
            {
                aspect.ClearAddedAndRemoved();

                B aspectMask = aspect.Bitmask;

                for (int j = 0; j < dirtyEntities.Count; ++j)
                {
                    Entity entity = dirtyEntityData[j];
                    B mask = entityMasks[entity.Index];
                    if (mask.Contains(aspect.Bitmask)) aspect.Add(entity);
                    else if (!mask.Contains(aspect.Bitmask)) aspect.Remove(entity);
                }

                for (int j = 0; j < entitiesToDestroy.Count; ++j)
                {
                    Entity entity = entitiesToDestroyData[j];
                    B mask = entityMasks[entity.Index];
                    if (mask.Contains(aspect.Bitmask)) aspect.Remove(entity);
                }
            }
            dirtyEntities.Clear();

            for (int j = 0; j < entitiesToDestroy.Count; ++j)
            {
                Entity entity = entitiesToDestroyData[j];
                int index = entity.Index;
                sparseComponents.RemoveAll(index);
                entityManager.Destroy(entity);
                entityMasks[index] = default(B);
            }
            entitiesToDestroy.Clear();
        }
    }
}
