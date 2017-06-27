// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic
{
    public class EventBus
    {
        private readonly List<IEventQueue> eventQueues = new List<IEventQueue>();

        private bool asyncMessagesAvailable;

        public EventBus() { }

        public void Register<T>(Action<T> listener) => GetOrCreateEventQueue<T>().Register(listener);
        public void Deregister<T>(Action<T> listener)
        {
            int typeIndex = GetTypeId(typeof(T));
            if (typeIndex >= eventQueues.Count) return;
            (eventQueues[typeIndex] as EventQueue<T>).DeRegister(listener);
        }

        public void Dispatch<T>(T evnt)
        {
            asyncMessagesAvailable = true;
            GetOrCreateEventQueue<T>().DispatchAsync(evnt);
        }

        public void Update()
        {
            while (asyncMessagesAvailable)
            {
                asyncMessagesAvailable = false;
                for (int i = 0; i < eventQueues.Count; ++i) eventQueues[i].DispatchEnquedEvents();
            }
        }

        private EventQueue<T> GetOrCreateEventQueue<T>()
        {
            int typeIndex = GetTypeId(typeof(T));
            if (typeIndex >= eventQueues.Count) eventQueues.Add(new EventQueue<T>());
            return eventQueues[typeIndex] as EventQueue<T>;
        }

        private class EventQueue<T> : IEventQueue
        {
            private readonly Queue<T> eventQueue = new Queue<T>();
            private Action<T> listeners;

            public void Register(Action<T> listener) => listeners += listener;
            public void DeRegister(Action<T> listener) => listeners -= listener;
            public void DispatchSync(T evt) => listeners?.Invoke(evt);
            public void DispatchAsync(T evt) => eventQueue.Enqueue(evt);

            public void DispatchEnquedEvents()
            {
                while (eventQueue.Count > 0) DispatchSync(eventQueue.Dequeue());
            }
        }

        private interface IEventQueue
        {
            void DispatchEnquedEvents();
        }

        private int GetTypeId(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
