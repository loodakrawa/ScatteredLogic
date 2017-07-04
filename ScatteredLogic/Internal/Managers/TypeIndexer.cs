// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic.Internal.Managers
{
    internal sealed class TypeIndexer
    {
        private readonly Dictionary<Type, int?> typeIndices = new Dictionary<Type, int?>();
        private readonly int maxTypes;

        public TypeIndexer(int maxTypes)
        {
            this.maxTypes = maxTypes;
        }

        public int GetIndex(Type type)
        {
            typeIndices.TryGetValue(type, out int? typeId);

            if (typeId.HasValue) return typeId.Value;

            typeId = typeIndices.Count;

            if (typeId >= maxTypes) throw new Exception("Number of types Exceeded: " + maxTypes);

            typeIndices[type] = typeId;

            return typeId.Value;
        }
    }
}
