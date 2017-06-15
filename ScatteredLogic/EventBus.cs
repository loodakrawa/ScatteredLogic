using ScatteredLogic.Internal;
using System;
using System.Collections.Generic;

namespace ScatteredLogic
{
    public class EventBus
    {
        private readonly List<IEventQueue> eventQueues = new List<IEventQueue>();
        private readonly TypeIndexer indexer = new TypeIndexer(int.MaxValue);

        private bool asyncMessagesAvailable;

        public void Register<T>(Action<T> listener) => GetOrCreateEventQueue<T>().Register(listener);
        public void Deregister<T>(Action<T> listener)
        {
            int typeIndex = indexer.GetIndex<T>();
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
                for (int i=0; i<eventQueues.Count; ++i) eventQueues[i].DispatchEnquedEvents();
            }
        }

        private EventQueue<T> GetOrCreateEventQueue<T>()
        {
            int typeIndex = indexer.GetIndex<T>();
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
    }
}
