namespace ScatteredGameExample.Parallel
{
    public interface ITask
    {
        void Run();
    }

    public interface ITaskCallback
    {
        void Wait();
    }

    public interface ITaskQueue
    {
        ITaskCallback Enqueue(ITask task);
    }
}
