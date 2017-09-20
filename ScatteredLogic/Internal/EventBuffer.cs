// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;
using System;

namespace ScatteredLogic.Internal
{
    internal sealed class EventBuffer<T> : IEventBuffer
    {
        private readonly int typeIndex;

        private int additionsCount;
        private readonly AddComponentEvent<T>[] additions;

        private int removalsCount;
        private readonly RemoveComponentEvent<T>[] removals;

        public EventBuffer(int size, int typeIndex)
        {
            this.typeIndex = typeIndex;

            additions = new AddComponentEvent<T>[size];
            removals = new RemoveComponentEvent<T>[size];
        }

        public void Add(AddComponentEvent<T> e)
        {
            additions[additionsCount++] = e;
        }

        public void Remove(RemoveComponentEvent<T> e)
        {
            removals[removalsCount++] = e;
        }

        private void Reset()
        {
            Array.Clear(additions, 0, additionsCount);
            additionsCount = 0;

            Array.Clear(removals, 0, removalsCount);
            removalsCount = 0;
        }

        public void Flush<B>(EntityWorld<B> entityWorld) where B : struct, IBitmask<B>
        {
            for (int i = 0; i < additionsCount; ++i)
            {
                AddComponentEvent<T> e = additions[i];
                entityWorld.AddComponent<T>(e.Entity, e.Component, typeIndex);
            }

            for (int i = 0; i < removalsCount; ++i)
            {
                RemoveComponentEvent<T> e = removals[i];
                entityWorld.RemoveComponent<T>(e.Entity, typeIndex);
            }

            Reset();
        }
    }

    internal interface IEventBuffer
    {
        void Flush<B>(EntityWorld<B> entityWorld) where B : struct, IBitmask<B>;
    }
}
