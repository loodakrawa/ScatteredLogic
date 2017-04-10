using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class IdPool
    {
        private int max;

        private Stack<int> pool = new Stack<int>();

        public int Get()
        {
            return pool.Count > 0 ? pool.Pop() : max++;
        }

        public void Return(int id)
        {
            pool.Push(id);
        }
    }
}
