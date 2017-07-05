// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ScatteredLogic
{
    public struct ArrayEnumerator<T> : IEnumerator<T>
    {
        private readonly T[] entities;
        private readonly int count;

        private T current;
        private int index;

        internal ArrayEnumerator(T[] entities, int count)
        {
            Debug.Assert(entities != null);
            Debug.Assert(count >= 0);

            this.entities = entities;
            this.count = count;

            index = 0;
            current = default(T);
        }

        public T Current => current;

        object IEnumerator.Current => current;

        public void Dispose() { }

        public bool MoveNext()
        {
            if (index >= count) return false;
            current = entities[index];
            ++index;
            return true;
        }

        public void Reset()
        {
            index = 0;
            current = default(T);
        }
    }
}
