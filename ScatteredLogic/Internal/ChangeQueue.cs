// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;

namespace ScatteredLogic.Internal
{
    internal sealed class ChangeQueue
    {
        private readonly int maxEvents;

        private readonly IEventBuffer[] buffers;

        public ChangeQueue(int maxComponentTypes, int maxEvents)
        {
            this.maxEvents = maxEvents;

            buffers = new IEventBuffer[maxComponentTypes];
        }

        public void AddComponent<T>(Entity entity, T component, int typeId)
        {
            GetBuffer<T>(typeId).Add(new AddComponentEvent<T>(entity, component));
        }

        public void RemoveComponent<T>(Entity entity, int typeId)
        {
            GetBuffer<T>(typeId).Remove(new RemoveComponentEvent<T>(entity));
        }

        public bool GetComponent<T>(Entity entity, int typeId, out T component) => GetBuffer<T>(typeId).Get(entity, out component);

        private EventBuffer<T> GetBuffer<T>(int typeId)
        {
            EventBuffer<T> buffer = (EventBuffer<T>)buffers[typeId];

            if (buffer == null)
            {
                buffer = new EventBuffer<T>(maxEvents, typeId);
                buffers[typeId] = buffer;
            }

            return buffer;
        }

        internal void Flush<B>(EntityWorld<B> entityWorld) where B : struct, IBitmask<B>
        {
            for (int i = 0; i < buffers.Length; ++i) buffers[i]?.Flush(entityWorld);
        }
    }
}
