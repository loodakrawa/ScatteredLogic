// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;
using ScatteredLogic.Internal.DataStructures;
using System;
using System.Diagnostics;

namespace ScatteredLogic.Internal
{
    internal sealed class Aspect<B> : IAspect where B : struct, IBitmask<B>
    {
        public B Bitmask { get; private set; }

        public int EntityCount => entities.Count;
        public Entity[] Entities => entities.Data;

        private readonly int maxEntities;

        private readonly SparseComponentArray sparseComponents;
        private readonly TypeIndexer typeIndexer;

        private readonly IntMap entityMap;
        private readonly IntMap typeMap;

        private readonly IArrayWrapper[] components;
        private readonly ArrayWrapper<Entity> entities;

        public Aspect(SparseComponentArray sparseComponents, TypeIndexer typeIndexer, int maxEntities, int maxComponentTypes, Type[] types)
        {
            Debug.Assert(maxEntities > 0);
            Debug.Assert(maxComponentTypes > 0);

            this.maxEntities = maxEntities;
            this.typeIndexer = typeIndexer;
            this.sparseComponents = sparseComponents;

            entityMap = new IntMap(maxEntities);
            typeMap = new IntMap(maxComponentTypes);

            entities = new ArrayWrapper<Entity>(maxEntities);
            components = new IArrayWrapper[types.Length];

            foreach (Type type in types)
            {
                int typeIndex = typeIndexer.GetIndex(type);
                int packedIndex = typeMap.Add(typeIndex);

                Type genericArrayWrapper = typeof(ArrayWrapper<>).MakeGenericType(type);
                IArrayWrapper array = (IArrayWrapper)Activator.CreateInstance(genericArrayWrapper, maxEntities);
                components[packedIndex] = array;

                Bitmask = Bitmask.Set(typeIndex);
            }
        }

        public T[] GetComponents<T>()
        {
            int typeIndex = typeIndexer.GetIndex(typeof(T));
            int packedTypeIndex = typeMap.GetPacked(typeIndex);
            return ((ArrayWrapper<T>)components[packedTypeIndex]).Data;
        }

        public void Add(Entity entity)
        {
            int sparseIndex = entity.Index;
            int packedIndex = entityMap.Add(sparseIndex);

            entities[packedIndex] = entity;

            if (packedIndex == entities.Count) ++entities.Count;

            for (int i = 0; i < components.Length; ++i)
            {
                int typeIndex = typeMap.GetSparse(i);
                IArrayWrapper array = components[i];
                IArrayWrapper sparseArray = sparseComponents.GetArrayWrapper(typeIndex);
                array.AddFrom(packedIndex, sparseArray, entity.Index);
                array.Count = entities.Count;
            }
        }

        public void Remove(Entity entity)
        {
            int index = entity.Index;

            if (!entityMap.Contains(index)) return;

            entityMap.Remove(index, out int lastPackedIndex, out int packedIndex);

            if (packedIndex != lastPackedIndex) entities.Swap(packedIndex, lastPackedIndex);
            entities.RemoveElementAt(lastPackedIndex);
            --entities.Count;

            for (int i = 0; i < components.Length; ++i)
            {
                IArrayWrapper array = components[i];
                if (packedIndex != lastPackedIndex) array.Swap(packedIndex, lastPackedIndex);
                array.RemoveElementAt(lastPackedIndex);
                array.Count = entities.Count;
            }
        }
    }
}
