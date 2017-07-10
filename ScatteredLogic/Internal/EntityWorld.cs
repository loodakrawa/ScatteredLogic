// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;
using ScatteredLogic.Internal.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ScatteredLogic.Internal
{
    internal sealed class EntityWorld<B> : IEntityWorld where B : struct, IBitmask<B>
    {
        public IArray<Handle> Entities => entities;

        private readonly int maxEntities;
        private readonly int maxComponentTypes;

        private readonly PackedArray<Handle> entities;
        private readonly B[] entityMasks;
        private readonly TypeIndexer typeIndexer;
        private readonly SparseComponentArray sparseComponents;
        private readonly HandleManager entityManager;

        private readonly List<Aspect<B>> aspects = new List<Aspect<B>>();

        private readonly PackedArray<Handle> dirtyEntities;
        private readonly ChangeQueue changeQueue;

        private readonly PackedArray<Handle> entitiesToDestroy;

        public EntityWorld(int maxEntities, int maxComponentTypes, int maxEvents)
        {
            this.maxEntities = maxEntities;
            this.maxComponentTypes = maxComponentTypes;

            entities = new PackedArray<Handle>(maxEntities);
            entityMasks = new B[maxEntities];

            typeIndexer = new TypeIndexer(maxComponentTypes);
            entityManager = new HandleManager(maxEntities);
            sparseComponents = new SparseComponentArray(maxComponentTypes, maxEntities);

            changeQueue = new ChangeQueue(maxComponentTypes, maxEvents);
            dirtyEntities = new PackedArray<Handle>(maxEntities);
            entitiesToDestroy = new PackedArray<Handle>(maxEntities);
        }

        public Handle CreateEntity()
        {
            lock (entityManager)
            {
                Handle entity = entityManager.Create();
                entities.Add(entity, entity.Index);
                entityMasks[entity.Index] = default(B);
                return entity;
            }
        }

        public void DestroyEntity(Handle entity)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);

            lock (entitiesToDestroy) entitiesToDestroy.Add(entity, entity.Index);
        }

        public bool ContainsEntity(Handle entity)
        {
            return entityManager.Contains(entity);
        }

        public void AddComponent<T>(Handle entity, T component)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);

            changeQueue.AddComponent<T>(entity, component, typeIndexer.GetIndex(typeof(T)));
        }

        public void AddComponent<T>(Handle entity, T component, int typeIndex)
        {
            int entityIndex = entity.Index;

            sparseComponents.Add(entity.Index, component, typeIndex);
            entityMasks[entityIndex] = entityMasks[entityIndex].Set(typeIndex);

            dirtyEntities.Add(entity, entityIndex);
        }

        public void RemoveComponent<T>(Handle entity)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);
            changeQueue.RemoveComponent<T>(entity, typeIndexer.GetIndex(typeof(T)));
        }

        public void RemoveComponent<T>(Handle entity, int typeIndex)
        {
            int entityIndex = entity.Index;

            sparseComponents.Remove(entity.Index, typeIndex);
            entityMasks[entityIndex] = entityMasks[entityIndex].Clear(typeIndex);

            dirtyEntities.Add(entity, entityIndex);
        }

        public T GetComponent<T>(Handle entity)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);

            IArray<T> array = sparseComponents.GetArray<T>(typeIndexer.GetIndex(typeof(T)));
            return array != null ? array[entity.Index] : default(T);
        }

        public IAspect CreateAspect(Type[] types)
        {
            Aspect<B> aspect = new Aspect<B>(sparseComponents, typeIndexer, maxEntities, maxComponentTypes, types);
            aspects.Add(aspect);
            return aspect;
        }

        public void Commit()
        {
            changeQueue.Flush(this);

            foreach (Aspect<B> aspect in aspects)
            {
                B aspectMask = aspect.Bitmask;

                for (int j = 0; j < dirtyEntities.Count; ++j)
                {
                    Handle entity = dirtyEntities[j];
                    B mask = entityMasks[entity.Index];
                    if (mask.Contains(aspect.Bitmask)) aspect.Add(entity);
                    else if (!mask.Contains(aspect.Bitmask)) aspect.Remove(entity);
                }

                for (int j = 0; j < entitiesToDestroy.Count; ++j)
                {
                    Handle entity = entitiesToDestroy[j];
                    B mask = entityMasks[entity.Index];
                    if (mask.Contains(aspect.Bitmask)) aspect.Remove(entity);
                }
            }
            dirtyEntities.Clear();

            for (int j = 0; j < entitiesToDestroy.Count; ++j)
            {
                Handle entity = entitiesToDestroy[j];
                entityManager.Destroy(entity);
                entityMasks[entity.Index] = default(B);
            }
            entitiesToDestroy.Clear();
        }
    }
}
