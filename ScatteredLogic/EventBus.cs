using System;
using System.Collections.Generic;

namespace ScatteredLogic
{
    public class EventBus
    {
        private readonly Dictionary<Type, IEventQueue> eventQueues = new Dictionary<Type, IEventQueue>();

        private bool asyncMessagesAvailable;

        public void Register<T>(Action<T> listener) => GetOrCreateEventQueue<T>().Register(listener);

        public void Deregister<T>(Action<T> listener) => GetEventQueue<T>(typeof(T))?.DeRegister(listener);

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
                foreach (IEventQueue ieq in eventQueues.Values) ieq.DispatchEnquedEvents();
            }
        }

        private EventQueue<T> GetEventQueue<T>(Type type)
        {
            IEventQueue eqObj;
            eventQueues.TryGetValue(type, out eqObj);
            return eqObj as EventQueue<T>;
        }

        private EventQueue<T> GetOrCreateEventQueue<T>()
        {
            Type type = typeof(T);
            EventQueue<T> eq = GetEventQueue<T>(type);
            if (eq == null)
            {
                eq = new EventQueue<T>();
                eventQueues[type] = eq;
            }
            return eq;
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
