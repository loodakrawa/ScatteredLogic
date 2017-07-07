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
        private PackedArray<Handle> dirtyEntities;
        private readonly B[] entityMasks;
        private readonly TypeIndexer typeIndexer;
        private readonly SparseComponentArray sparseComponents;
        private readonly HandleManager entityManager;

        private readonly List<Aspect<B>> aspects = new List<Aspect<B>>();

        private readonly object locker = new object();

        public EntityWorld(int maxEntities, int maxComponentTypes)
        {
            this.maxEntities = maxEntities;
            this.maxComponentTypes = maxComponentTypes;

            entities = new PackedArray<Handle>(maxEntities);
            dirtyEntities = new PackedArray<Handle>(maxEntities);
            entityMasks = new B[maxEntities];

            typeIndexer = new TypeIndexer(maxComponentTypes);
            entityManager = new HandleManager(maxEntities);
            sparseComponents = new SparseComponentArray(maxComponentTypes, maxEntities);
        }

        public Handle CreateEntity()
        {
            lock (locker)
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
            int index = entity.Index;

            lock (locker)
            {
                entities.Remove(index);
                sparseComponents.RemoveAll(index);
                entityManager.Destroy(entity);
                entityMasks[index] = default(B);

                if (!dirtyEntities.Contains(index)) dirtyEntities.Add(entity, index);
            }
        }

        public bool ContainsEntity(Handle entity)
        {
            return entityManager.Contains(entity);
        }

        public void AddComponent<T>(Handle entity, T component)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);
            int entityIndex = entity.Index;

            lock (locker)
            {
                int typeIndex = typeIndexer.GetIndex(typeof(T));
                sparseComponents.Add(entity.Index, component, typeIndex);
                entityMasks[entityIndex] = entityMasks[entityIndex].Set(typeIndex);

                dirtyEntities.Add(entity, entityIndex);
            }
        }

        public void RemoveComponent<T>(Handle entity)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);
            int entityIndex = entity.Index;

            lock (locker)
            {
                int typeIndex = typeIndexer.GetIndex(typeof(T));
                sparseComponents.Remove(entity.Index, typeIndex);
                entityMasks[entityIndex] = entityMasks[entityIndex].Clear(typeIndex);

                dirtyEntities.Add(entity, entityIndex);
            }
        }

        public T GetComponent<T>(Handle entity)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);

            IArray<T> array = sparseComponents.GetArray<T>(typeIndexer.GetIndex(typeof(T)));
            return array[entity.Index];
        }

        public IAspect CreateAspect(Type[] types)
        {
            Aspect<B> aspect = new Aspect<B>(sparseComponents, typeIndexer, maxEntities, maxComponentTypes, types);
            aspects.Add(aspect);
            return aspect;
        }

        public void Step()
        {
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
            }
            dirtyEntities.Clear();
        }
    }
}
