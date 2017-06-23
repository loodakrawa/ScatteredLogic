// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class TypeIndexer
    {
        private readonly Dictionary<Type, int?> componentIndexes = new Dictionary<Type, int?>();
        private readonly int maxComponents;

        public TypeIndexer(int maxComponents)
        {
            this.maxComponents = maxComponents;
        }

        public int GetTypeId(Type type)
        {
            int? index;
            componentIndexes.TryGetValue(type, out index);

            if (index.HasValue) return index.Value;

            index = componentIndexes.Count;

            if (index >= maxComponents) throw new Exception("Number of components Exceeded: " + maxComponents);

            componentIndexes[type] = index;

            return index.Value;
        }
    }
}
