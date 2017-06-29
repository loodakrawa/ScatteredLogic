// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal.Managers
{
    internal sealed class TypeIndexManager
    {
        private readonly Dictionary<Type, int?> componentIndexes = new Dictionary<Type, int?>();
        private readonly int maxComponentTypes;

        public TypeIndexManager(int maxComponents)
        {
            this.maxComponentTypes = maxComponents;
        }

        public int GetIndex(Type type)
        {
            int? index;
            componentIndexes.TryGetValue(type, out index);

            if (index.HasValue) return index.Value;

            index = componentIndexes.Count;

            if (index >= maxComponentTypes) throw new Exception("Number of components Exceeded: " + maxComponentTypes);

            componentIndexes[type] = index;

            return index.Value;
        }
    }
}
