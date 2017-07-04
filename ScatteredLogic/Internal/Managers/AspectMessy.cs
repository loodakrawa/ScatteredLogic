// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.DataStructures;
using System;

namespace ScatteredLogic.Internal.Managers
{
    internal sealed class AspectMessy : IArray<Handle>
    {
        public int Count => entityCount;
        public Handle this[int i] { get => entities[i]; }
        public ArrayEnumerator<Handle> GetEnumerator() => new ArrayEnumerator<Handle>(entities, entityCount);

        private readonly SparseComponentArray componentManager;

        private readonly Handle[] entities;
        private readonly int[] indices;
        private int entityCount;

        private readonly IArrayWrapper[] components;
        private readonly int[] componentTypeIndices;
        private int componentTypeCount;

        public AspectMessy(SparseComponentArray componentManager, int maxEntities, int typeCount)
        {
            this.componentManager = componentManager;

            entities = new Handle[maxEntities];
            indices = new int[maxEntities];
            for (int i = 0; i < maxEntities; ++i) indices[i] = -1;

            components = new IArrayWrapper[typeCount];
            componentTypeIndices = new int[typeCount];
        }

        public void RegisterType(Type type, int typeId)
        {
            Type genericType = typeof(ArrayWrapper<>).MakeGenericType(type);
            IArrayWrapper array = Activator.CreateInstance(genericType, entities.Length) as IArrayWrapper;
            components[componentTypeCount] = array;
            componentTypeIndices[componentTypeCount] = typeId;
            ++componentTypeCount;
        }

        public void AddComponent<T>(Handle entity, T component, int typeId)
        {
            int packedIndex = indices[entity.Index];
            ArrayWrapper<T> array = components[typeId] as ArrayWrapper<T>;
            array[packedIndex] = component;
        }

        public void Add(Handle entity)
        {
            int index = entity.Index;
            int packedIndex;

            if (ContainsEntity(entity))
            {
                packedIndex = indices[index];
            }
            else
            {
                packedIndex = entityCount;
                ++entityCount;
            }

            entities[packedIndex] = entity;
            indices[index] = packedIndex;

            for (int i = 0; i < components.Length; ++i)
            {
                IArrayWrapper array = components[i];
                int typeId = componentTypeIndices[i];
                IArrayWrapper globalComponents = componentManager.GetArrayWrapper(typeId);
                if(globalComponents != null) array.AddFrom(packedIndex, globalComponents, index);
            }

        }

        public void Remove(Handle entity)
        {
            int index = entity.Index;
            int packedIndex = indices[index];

            // get last element in packed array
            int lastPackedIndex = entityCount - 1;

            // if the element to remove is not the last
            if (packedIndex != lastPackedIndex)
            {
                // swap last element and element to remove
                Handle lastEntity = entities[lastPackedIndex];
                entities[packedIndex] = lastEntity;
                indices[lastEntity.Index] = packedIndex;

                // swap and remove last
                foreach (IArrayWrapper components in components) components.SwapAndRemove(packedIndex, lastPackedIndex);
            }
            else
            {
                // remove entity from all component arrays
                foreach (IArrayWrapper components in components) components.RemoveElementAt(packedIndex);
            }

            indices[index] = -1;

            --entityCount;
        }

        public bool ContainsEntity(Handle handle)
        {
            int index = handle.Index;
            return indices[index] != -1;
        }

        public IArray<T> Get<T>(int typeId)
        {
            for (int i = 0; i < componentTypeCount; ++i) if (componentTypeIndices[i] == typeId) return components[i] as ArrayWrapper<T>;
            return null;
        }
    }
}
