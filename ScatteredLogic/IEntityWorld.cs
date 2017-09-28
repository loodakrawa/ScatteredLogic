// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.


using System;

namespace ScatteredLogic
{
    public interface IEntityWorld
    {
        int EntityCount { get; }
        Entity[] Entities { get; }

        Entity CreateEntity();
        void DestroyEntity(Entity entity);
        bool ContainsEntity(Entity entity);

        void AddComponent<T>(Entity entity, T component);
        void RemoveComponent<T>(Entity entity);
        T GetComponent<T>(Entity entity);
        T[] GetComponents<T>();

        void Commit();

        IAspect CreateAspect(Type[] types);
    }
}
