// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic
{
    public interface ISystem<E> where E : struct, IEquatable<E>
    {
        IEnumerable<Type> RequiredComponents { get; }
        IEntityManager<E> EntityManager { get; set; }
        SetEnumerable<E> Entities { get; set; }
        EventBus EventBus { get; set; }

        void Added();
        void Removed();

        void EntityAdded(E entity);
        void EntityRemoved(E entity);

        void Update(float deltaTime);
    }
}
