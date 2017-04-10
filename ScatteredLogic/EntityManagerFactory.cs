// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using ScatteredLogic.Internal;
using System;

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
        public static IEntityManager<Entity> Create(BitmaskSize type)
        {
            EntityFactory ef = new EntityFactory();
            IEntityManager<Entity> em = InternalCreate(type, ef);
            ef.EntityManager = em;
            return em;
        }

        public static IEntityManager<E> Create<E>(BitmaskSize type, IEntityFactory<E> entityFactory) where E : struct, IEquatable<E>
        {
            return InternalCreate(type, entityFactory);
        }

        private static IEntityManager<E> InternalCreate<E>(BitmaskSize type, IEntityFactory<E> entityFactory) where E : struct, IEquatable<E>
        {
            switch (type)
            {
                case BitmaskSize.Bit32: return new EntityManager<E, Bitmask32>(32, entityFactory);
                case BitmaskSize.Bit64: return new EntityManager<E, Bitmask64>(64, entityFactory);
                case BitmaskSize.Bit128: return new EntityManager<E, Bitmask32>(128, entityFactory);
                default: return null;
            }
        }
    }
}
