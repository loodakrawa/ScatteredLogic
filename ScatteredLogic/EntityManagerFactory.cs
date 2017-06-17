// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using ScatteredLogic.Internal;

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
        public static IEntityManager Create(BitmaskSize type)
        {
            IEntityManager em = InternalCreate(type);
            return em;
        }

        private static IEntityManager InternalCreate(BitmaskSize type)
        {
            switch (type)
            {
                case BitmaskSize.Bit32: return new EntityManager<Bitmask32>(32);
                case BitmaskSize.Bit64: return new EntityManager<Bitmask64>(64);
                case BitmaskSize.Bit128: return new EntityManager<Bitmask128>(128);
                default: return null;
            }
        }
    }
}
