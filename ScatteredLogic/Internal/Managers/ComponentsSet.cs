// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.DataStructures;
using System;

namespace ScatteredLogic.Internal.Managers
{
    internal sealed class ComponentsSet
    {
        private readonly ComponentManager componentManager;

        private readonly Handle[] entities;
        private readonly int[] indices;
        private int entityCount;

        private readonly IArrayWrapper[] componentArrays;
        private readonly int[] componentTypeIndices;
        private int componentTypeCount;

        public ComponentsSet(ComponentManager componentManager, int maxEntities, int typeCount)
        {
            this.componentManager = componentManager;

            entities = new Handle[maxEntities];
            indices = new int[maxEntities];
            for (int i = 0; i < maxEntities; ++i) indices[i] = -1;

            componentArrays = new IArrayWrapper[typeCount];
            componentTypeIndices = new int[typeCount];
        }

        public void RegisterType(Type type, int typeId)
        {
            IArrayWrapper components = componentArrays[componentTypeCount];

            Type genericType = typeof(ArrayWrapper<>).MakeGenericType(type);
            components = Activator.CreateInstance(genericType, entities.Length) as IArrayWrapper;
            componentArrays[componentTypeCount] = components;
            componentTypeIndices[componentTypeCount] = typeId;
            ++componentTypeCount;
        }

        public void UpdateComponent<T>(Handle entity, T component, int typeId)
        {
            int packedIndex = indices[entity.Index];
            if (packedIndex >= componentArrays.Length) return;
            ArrayWrapper<T> components = componentArrays[typeId] as ArrayWrapper<T>;
            components[packedIndex] = component;
        }

        public void Add(Handle entity)
        {
            int index = entity.Index;
            int packedIndex = entityCount;

            entities[packedIndex] = entity;
            indices[index] = packedIndex;

            for (int i = 0; i < componentArrays.Length; ++i)
            {
                IArrayWrapper localComponents = componentArrays[i];
                int typeId = componentTypeIndices[i];
                IArrayWrapper globalComponents = componentManager.Get(typeId);
                if(globalComponents != null) localComponents.AddFrom(packedIndex, globalComponents, index);
            }

            ++entityCount;
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
                foreach (IArrayWrapper components in componentArrays) components.SwapAndRemove(packedIndex, lastPackedIndex);
            }
            else
            {
                // remove entity from all component arrays
                foreach (IArrayWrapper components in componentArrays) components.RemoveElementAt(packedIndex);
            }

            indices[index] = -1;

            --entityCount;
        }

        public bool ContainsEntity(Handle handle)
        {
            int index = handle.Index;
            if (index >= entityCount) return false;
            return indices[index] != -1;
        }

        public IArray<T> Get<T>(int typeId)
        {
            return componentArrays[typeId] as ArrayWrapper<T>;
        }
    }
}
