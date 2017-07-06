using System;
using System.Threading;

namespace ScatteredGameExample.Parallel
{
    public class TaskCallback : ITaskCallback
    {
        public readonly WaitCallback WaitCallback;

        private readonly ThreadPoolTaskQueue taskQueue;
        private readonly ManualResetEvent resetEvent = new ManualResetEvent(false);

        private ITask task;

        public TaskCallback(ThreadPoolTaskQueue taskQueue)
        {
            this.taskQueue = taskQueue;
            WaitCallback = Execute;
        }

        public void Wait()
        {
            resetEvent.WaitOne();
        }

        public void Reset(ITask task)
        {
            this.task = task;
            resetEvent.Reset();
        }

        private void Execute(object state)
        {
            task.Run();
            resetEvent.Set();
            taskQueue.Return(this);
        }
    }
}