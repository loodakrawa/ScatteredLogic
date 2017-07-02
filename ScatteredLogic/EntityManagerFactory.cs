// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal;
using ScatteredLogic.Internal.Bitmasks;

namespace ScatteredLogic
{
    public enum BitmaskSize
    {
        Bit32,
        Bit64,
        Bit128
    }

    public static class EntityManagerFactory
    {
        public static IEntityWorld CreateEntityWorld(int maxEntities, int maxComponentTypes)
        {
            return new EntityWorld(maxEntities, maxComponentTypes);
        }

        public static IGroupedEntityWorld CreateGroupedEntityWorld(int maxEntities, BitmaskSize bitmaskSize)
        {
            switch (bitmaskSize)
            {
                case BitmaskSize.Bit32: return new GroupedEntityWorld<Bitmask32>(CreateEntityWorld(maxEntities, 32), maxEntities, 32);
                case BitmaskSize.Bit64: return new GroupedEntityWorld<Bitmask64>(CreateEntityWorld(maxEntities, 64), maxEntities, 64);
                case BitmaskSize.Bit128: return new GroupedEntityWorld<Bitmask128>(CreateEntityWorld(maxEntities, 128), maxEntities, 128);
                default: return null;
            }

        }
    }
}
