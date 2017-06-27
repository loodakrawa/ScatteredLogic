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
        public static IEntityWorld CreateEntityManager(BitmaskSize type, int maxEntities)
        {
            switch (type)
            {
                case BitmaskSize.Bit32: return new EntityWorld<Bitmask32>(32, maxEntities);
                case BitmaskSize.Bit64: return new EntityWorld<Bitmask64>(64, maxEntities);
                case BitmaskSize.Bit128: return new EntityWorld<Bitmask128>(128, maxEntities);
                default: return null;
            }
        }

        public static IEntitySystemManager CreateEntitySystemManager(BitmaskSize type, int maxEntities)
        {
            switch (type)
            {
                case BitmaskSize.Bit32: return new EntitySystemManager<Bitmask32>(32, maxEntities);
                case BitmaskSize.Bit64: return new EntitySystemManager<Bitmask64>(64, maxEntities);
                case BitmaskSize.Bit128: return new EntitySystemManager<Bitmask128>(128, maxEntities);
                default: return null;
            }
        }
    }
}
