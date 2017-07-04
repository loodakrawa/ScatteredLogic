// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;
using ScatteredLogic.Internal.DataStructures;
using System;

namespace ScatteredLogic.Internal.Managers
{
    internal sealed class Aspect<B> : IArray<Handle> where B : struct, IBitmask<B>
    {
        public B Bitmask { get; private set; }

        public int Count => entities.Count;
        public Handle this[int i] { get => entities[i]; }
        public ArrayEnumerator<Handle> GetEnumerator() => entities.GetEnumerator();

        private readonly SparseComponentArray componentManager;
        private readonly PackedHandleArray entities;
        private readonly PackedComponentArray components;

        public Aspect(SparseComponentArray componentManager, int maxEntities, int maxComponentTypes)
        {
            this.componentManager = componentManager;

            entities = new PackedHandleArray(maxEntities);
            components = new PackedComponentArray(maxComponentTypes, maxEntities);
        }

        public void RegisterType(Type type, int typeIndex)
        {
            components.RegisterType(type, typeIndex);
            Bitmask = Bitmask.Set(typeIndex);
        }

        public void AddComponent<T>(Handle entity, T component, int typeIndex)
        {
            components.AddComponent(entity.Index, component, typeIndex);
        }

        public void Remove(Handle entity)
        {
            components.Remove(entity.Index);
        }

        public bool Contains(Handle entity)
        {
            return entities.Contains(entity);
        }

        public IArray<T> GetArray<T>(int typeIndex)
        {
            return components.GetArray<T>(typeIndex);
        }

        public void Add(Handle entity)
        {
            int packedIndex = components.Add(entity.Index);

            for (int i = 0; i < components.componentTypeCount; ++i)
            {
                int typeIndex = components.componentPackedIndices[i];
                IArrayWrapper packedArray = components.GetArrayWrapper(typeIndex);
                IArrayWrapper sparseArray = componentManager.GetArrayWrapper(typeIndex);
                packedArray.AddFrom(packedIndex, sparseArray, entity.Index);
            }
        }


    }
}
