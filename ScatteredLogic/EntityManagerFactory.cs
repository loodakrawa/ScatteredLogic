// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using ScatteredLogic.Internal;
using ScatteredLogic.Internal.Bitmask;

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
        private const int DefaultInitialSize = 64;
        private const int DefaultGrowthSize = 32;

        public static IEntitySystemManager CreateEntitySystemManager(BitmaskSize type = BitmaskSize.Bit32, int initialSize = DefaultInitialSize, int growthSize = DefaultGrowthSize)
        {
            switch (type)
            {
                case BitmaskSize.Bit32: return new EntitySystemManager<Bitmask32>(32, initialSize, growthSize);
                case BitmaskSize.Bit64: return new EntitySystemManager<Bitmask64>(64, initialSize, growthSize);
                case BitmaskSize.Bit128: return new EntitySystemManager<Bitmask128>(128, initialSize, growthSize);
                default: return null;
            }
        }
    }
}
