using System.Collections.Generic;
using System.Threading;

namespace ScatteredGameExample.Parallel
{
    public sealed class CustomThreadPool
    {
        private readonly List<Thread> threads = new List<Thread>();
        private readonly Queue<WaitCallback> tasks = new Queue<WaitCallback>();

        public CustomThreadPool(int threadCount)
        {
            for (int i = 0; i < threadCount; ++i) SpawnThread();
        }

        private void SpawnThread()
        {
            Thread thread = new Thread(RunTasks);
            threads.Add(thread);
            thread.IsBackground = true;
            thread.Start();
        }

        public void EnqueueTask(WaitCallback task)
        {
            lock (tasks)
            {
                tasks.Enqueue(task);
                Monitor.Pulse(tasks);
            }
        }

        private void RunTasks()
        {
            while (true)
            {
                WaitCallback task;
                lock (tasks)
                {
                    while (tasks.Count == 0) Monitor.Wait(tasks);
                    task = tasks.Dequeue();
                }
                task(null);
            }
        }
    }
}
