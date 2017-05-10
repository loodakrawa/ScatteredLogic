using System;
using System.Collections.Generic;

namespace ScatteredGameExample
{
    public class EventBus
    {
        private readonly IDictionary<Type, object> listeners = new Dictionary<Type, object>();

        public void Register<T>(Action<T> listener)
        {
            Action<T> action = GetAction<T>();
            action = action + listener;
            listeners[typeof(T)] = action;
        }

        public void DeRegister<T>(Action<T> listener)
        {
            Action<T> action = GetAction<T>();

            if (action == null) return;

            action = action - listener;
            listeners[typeof(T)] = action;
        }

        public void Dispatch<T>(T evnt)
        {
            GetAction<T>()?.Invoke(evnt);
        }

        private Action<T> GetAction<T>()
        {
            Type type = typeof(T);

            object action;
            listeners.TryGetValue(type, out action);

            return action as Action<T>;
        }
    }
}
