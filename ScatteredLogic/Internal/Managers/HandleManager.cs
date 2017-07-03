using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal.Managers
{
    internal class HandleManager
    {
        private readonly Handle[] handles;
        private readonly Queue<int> freeIndices;

        private readonly int maxHandles;

        public HandleManager(int maxHandles)
        {
            if(maxHandles > Handle.MaxIndex) throw new Exception("Max possible number: " + Handle.MaxIndex);

            this.maxHandles = maxHandles;

            handles = new Handle[maxHandles];
            freeIndices = new Queue<int>(maxHandles);

            for (int i = 0; i < maxHandles; ++i)
            {
                freeIndices.Enqueue(i);
                handles[i] = new Handle(i);
            }
        }

        public virtual Handle Create()
        {
            if (freeIndices.Count == 0) throw new Exception("Max number of handles reached: " + maxHandles);

            int index = freeIndices.Dequeue();
            Handle handle = handles[index].IncrementVersion();
            handles[index] = handle;

            return handle;
        }

        public virtual void Destroy(Handle handle)
        {
            int index = handle.Index;
            freeIndices.Enqueue(index);
        }

        public bool Contains(Handle handle)
        {
            int index = handle.Index;
            return index < maxHandles && handles[index].Version == handle.Version;
        }
    }
}
