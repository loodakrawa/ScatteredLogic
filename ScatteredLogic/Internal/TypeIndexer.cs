﻿// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class TypeIndexer
    {
        private Dictionary<Type, int?> componentIndexes = new Dictionary<Type, int?>();

        public int GetIndex(object obj) => GetIndex(obj.GetType());
        public int GetIndex<T>() => GetIndex(typeof(T));

        private readonly int max;

        public TypeIndexer(int max)
        {
            this.max = max;
        }

        public int GetIndex(Type type)
        {
            int? index;
            componentIndexes.TryGetValue(type, out index);

            if (index.HasValue) return index.Value;

            index = componentIndexes.Count;

            if (index >= max) throw new Exception("Number of components Exceeded: " + max);

            componentIndexes[type] = index;

            return index.Value;
        }
    }
}
