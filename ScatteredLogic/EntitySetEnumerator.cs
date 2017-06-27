﻿// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System.Collections;
using System.Collections.Generic;

namespace ScatteredLogic
{
    public struct EntitySetEnumerator : IEnumerator<Handle>
    {
        private readonly Handle[] entities;
        private readonly int count;

        private Handle current;
        private int index;

        internal EntitySetEnumerator(Handle[] entities, int count)
        {
            this.entities = entities;
            this.count = count;

            index = 0;
            current = default(Handle);
        }

        public Handle Current => current;

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
            current = default(Handle);
        }
    }
}
