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

        private readonly PackedHandleArray entities;
        private PackedHandleArray dirtyEntitiesWrite;
        private PackedHandleArray dirtyEntitiesRead;
        private readonly B[] entityMasks;
        private readonly TypeIndexer typeIndexer;
        private readonly SparseComponentArray sparseComponents;
        private readonly HandleManager entityManager;

        private readonly Aspect<B>[] aspects;
        private readonly Handle[] aspectHandles;
        private int aspectCount;

        private readonly object locker = new object();

        public EntityWorld(int maxEntities, int maxComponentTypes, int maxAspects)
        {
            this.maxEntities = maxEntities;
            this.maxComponentTypes = maxComponentTypes;

            entities = new PackedHandleArray(maxEntities);
            dirtyEntitiesWrite = new PackedHandleArray(maxEntities);
            dirtyEntitiesRead = new PackedHandleArray(maxEntities);
            entityMasks = new B[maxEntities];

            typeIndexer = new TypeIndexer(maxComponentTypes);
            entityManager = new HandleManager(maxEntities);
            sparseComponents = new SparseComponentArray(maxComponentTypes, maxEntities);

            aspects = new Aspect<B>[maxAspects];
            aspectHandles = new Handle[maxAspects];
        }

        public Handle CreateEntity()
        {
            lock(locker)
            {
                Handle entity = entityManager.Create();
                entities.Add(entity);
                entityMasks[entity.Index] = default(B);
                return entity;
            }
        }

        public void DestroyEntity(Handle entity)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);
            int index = entity.Index;

            lock(locker)
            {
                entities.Remove(entity);
                sparseComponents.RemoveAll(index);
                entityManager.Destroy(entity);
                entityMasks[index] = default(B);

                if (!dirtyEntitiesWrite.Contains(entity)) dirtyEntitiesWrite.Add(entity);
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

            lock(locker)
            {
                int typeIndex = typeIndexer.GetIndex(typeof(T));
                sparseComponents.Add(entity.Index, component, typeIndex);
                entityMasks[entityIndex] = entityMasks[entityIndex].Set(typeIndex);

                if (!dirtyEntitiesWrite.Contains(entity)) dirtyEntitiesWrite.Add(entity);
            }
        }

        public void RemoveComponent<T>(Handle entity)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);
            int entityIndex = entity.Index;

            lock(locker)
            {
                int typeIndex = typeIndexer.GetIndex(typeof(T));
                sparseComponents.Remove(entity.Index, typeIndex);
                entityMasks[entityIndex] = entityMasks[entityIndex].Clear(typeIndex);

                if (!dirtyEntitiesWrite.Contains(entity)) dirtyEntitiesWrite.Add(entity);
            }
        }

        public T GetComponent<T>(Handle entity)
        {
            Debug.Assert(entityManager.Contains(entity), "Entity not managed: " + entity);

            IArray<T> array = sparseComponents.GetArray<T>(typeIndexer.GetIndex(typeof(T)));
            return array[entity.Index];
        }

        public Handle CreateAspect(IEnumerable<Type> types, string name)
        {
            Aspect<B> aspect = new Aspect<B>(sparseComponents, maxEntities, maxComponentTypes);
            aspect.Name = name;
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

        public void Step()
        {
            PackedHandleArray tmp = dirtyEntitiesRead;
            dirtyEntitiesRead = dirtyEntitiesWrite;
            dirtyEntitiesWrite = tmp;
            dirtyEntitiesWrite.Clear();
        }

        public void UpdateAspect(Handle handle)
        {
            int index = handle.Index;
            Aspect<B> aspect = aspects[index];
            B aspectMask = aspect.Bitmask;

            for (int j = 0; j < dirtyEntitiesRead.Count; ++j)
            {
                Handle entity = dirtyEntitiesRead[j];
                B mask = entityMasks[entity.Index];
                if (mask.Contains(aspect.Bitmask)) aspect.Add(entity);
                else if (!mask.Contains(aspect.Bitmask) && aspect.Contains(entity)) aspect.Remove(entity);
            }
        }
    }
}
