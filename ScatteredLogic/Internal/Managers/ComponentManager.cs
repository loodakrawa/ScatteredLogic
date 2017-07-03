// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.DataStructures;

namespace ScatteredLogic.Internal.Managers
{
    internal sealed class ComponentManager
    {
        private readonly int maxComponents;

        /*
                     |  0  |  1  |  2  |  3  |
          | Position |  X  |  X  |     |  X  |
          | Velocity |     |  X  |     |     |
          | Texture  |  X  |     |     |     |
        */
        private readonly IArrayWrapper[] componentArrays;

        public ComponentManager(int maxComponentTypes, int maxComponents)
        {
            componentArrays = new IArrayWrapper[maxComponentTypes];
            this.maxComponents = maxComponents;
        }

        public void RemoveAll(int index)
        {
            for (int i = 0; i < componentArrays.Length; ++i) componentArrays[i]?.RemoveElementAt(index);
        }

        public void Add<T>(int index, T component, int typeId)
        {
            ArrayWrapper<T> components = componentArrays[typeId] as ArrayWrapper<T>;
            if (components == null)
            {
                components = new ArrayWrapper<T>(maxComponents);
                componentArrays[typeId] = components;
            }

            components[index] = component;
        }

        public void Remove(int index, int typeId)
        {
            componentArrays[typeId]?.RemoveElementAt(index);
        }

        public T Get<T>(int index, int typeId)
        {
            ArrayWrapper<T> components = componentArrays[typeId] as ArrayWrapper<T>;
            return components != null ? components[index] : default(T);
        }

        public IArrayWrapper Get(int typeId)
        {
            return componentArrays[typeId];
        }
    }
}
