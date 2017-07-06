using System.Collections.Generic;

namespace ScatteredGameExample.Parallel
{
    public class ThreadPoolTaskQueue : ITaskQueue
    {
        private Stack<TaskCallback> callbacks = new Stack<TaskCallback>();
        private CustomThreadPool ctp;

        public ThreadPoolTaskQueue(int threadCount)
        {
            ctp = new CustomThreadPool(threadCount);
        }

        public ITaskCallback Enqueue(ITask task)
        {
            TaskCallback jc;
            lock (callbacks) jc = callbacks.Count > 0 ? callbacks.Pop() : new TaskCallback(this);

            jc.Reset(task);

            ctp.EnqueueTask(jc.WaitCallback);

            return jc;
        }

        internal void Return(TaskCallback callback)
        {
            lock(callbacks) callbacks.Push(callback);
        }
    }
}
