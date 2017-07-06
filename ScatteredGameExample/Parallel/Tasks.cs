namespace ScatteredGameExample.Parallel
{
    public static class Tasks
    {
        public static ITaskQueue TaskQueue { get; set; } = new SequentialTaskQueue();

        public static ITaskCallback Enqueue(ITask task) => TaskQueue.Enqueue(task);

        private class SequentialTaskQueue : ITaskQueue
        {
            private static readonly ITaskCallback dummyCallback = new DummyTaskCallback();

            public ITaskCallback Enqueue(ITask task)
            {
                task.Run();
                return dummyCallback;
            }
        }

        private class DummyTaskCallback : ITaskCallback
        {
            public void Wait() { }
        }
    }
}
