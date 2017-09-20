// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal.Bitmasks;

namespace ScatteredLogic.Internal
{
    internal class ChangeQueue
    {
        private readonly int maxEvents;

        private readonly object[] locks;
        private readonly IEventBuffer[] buffers;

        public ChangeQueue(int maxComponentTypes, int maxEvents)
        {
            this.maxEvents = maxEvents;

            locks = new object[maxComponentTypes];
            for (int i = 0; i < maxComponentTypes; ++i) locks[i] = new object();

            buffers = new IEventBuffer[maxComponentTypes];
        }

        public void AddComponent<T>(Entity entity, T component, int typeId)
        {
            lock (locks[typeId]) GetBuffer<T>(typeId).Add(new AddComponentEvent<T>(entity, component));
        }

        public void RemoveComponent<T>(Entity entity, int typeId)
        {
            lock (locks[typeId]) GetBuffer<T>(typeId).Remove(new RemoveComponentEvent<T>(entity));
        }

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
