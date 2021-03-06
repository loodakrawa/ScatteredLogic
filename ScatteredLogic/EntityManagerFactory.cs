﻿// Copyright (C) The original author or authors
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
        public static IEntityWorld CreateEntityWorld(int maxEntities, int maxEvents, BitmaskSize bitmaskSize)
        { 
            switch (bitmaskSize)
            {
                case BitmaskSize.Bit32: return new EntityWorld<Bitmask32>(maxEntities, maxEvents, 32);
                case BitmaskSize.Bit64: return new EntityWorld<Bitmask32>(maxEntities, maxEvents, 64);
                case BitmaskSize.Bit128: return new EntityWorld<Bitmask32>(maxEntities, maxEvents, 128);
                default: return null;
            }

        }
    }
}
