// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace ScatteredLogic
{
    public struct SetEnumerable<T>
    {
        private readonly HashSet<T> data;

        public SetEnumerable(HashSet<T> data)
        {
            this.data = data;
        }

        public int Count => data.Count;
        public HashSet<T>.Enumerator GetEnumerator() => data.GetEnumerator();
    }
}
