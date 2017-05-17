using System;
using System.Collections.Generic;

namespace ScatteredLogic
{
    public class EventBus
    {
        private readonly Dictionary<object, Action<object>> delegateMappings = new Dictionary<object, Action<object>>();
        private readonly Dictionary<Type, Action<object>> listeners = new Dictionary<Type, Action<object>>();
        private readonly Queue<object> queuedEvents = new Queue<object>();

        public void Register<T>(Action<T> listener)
        {
            Action<object> convertedListener = x => listener((T) x);
            delegateMappings[listener] = convertedListener;

            Type type = typeof(T);
            Action<object> actions = GetAction(type);
            listeners[type] = actions + convertedListener;
        }

        public void Deregister<T>(Action<T> listener)
        {
            Action<object> convertedListener;
            delegateMappings.TryGetValue(listener, out convertedListener);

            if (convertedListener == null) return;

            Type type = typeof(T);
            Action<object> actions = GetAction(type);
            if (actions != null) listeners[type] = actions - convertedListener;
        }

        public void DispatchSync<T>(T evnt) => GetAction(evnt.GetType())?.Invoke(evnt);
        public void DispatchAsync<T>(T evnt) => queuedEvents.Enqueue(evnt);     

        public void Update()
        {
            while (queuedEvents.Count > 0) DispatchSync(queuedEvents.Dequeue());
        }

        private Action<object> GetAction(Type type)
        {
            Action<object> action;
            listeners.TryGetValue(type, out action);

            return action;
        }
    }
}
