// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic
{
    public interface IArray<T>
    {
        T this[int i] { get; }
    }

    public interface IEntitySet
    {
        int Count { get; }
        Entity this[int i] { get; }
        bool Contains(Entity entity);
        EntitySetEnumerator GetEnumerator();
    }

    public interface IEntityManager
    {
        IEntitySet Entities { get; }

        Entity CreateEntity();
        void DestroyEntity(Entity entity);
        bool ContainsEntity(Entity entity);

        void AddComponent<T>(Entity entity, T component);
        void AddComponent(Entity entity, object component, Type type);

        void RemoveComponent<T>(Entity entity);
        void RemoveComponent(Entity entity, Type type);

        T GetComponent<T>(Entity entity);
        object GetComponent(Entity entity, Type type);

        IArray<T> GetComponents<T>();
    }

    public interface IEntitySystemManager : IEntityManager
    {
        void AddSystem(ISystem system);
        void RemoveSystem(ISystem system);

        void Update();
    }

    public interface ISystem
    {
        IEnumerable<Type> RequiredComponents { get; }
        IEntitySystemManager EntityManager { get; set; }
        IEntitySet Entities { get; set; }
        ISystemInfo Info { get; set; }

        void Added();
        void Removed();

        void EntityAdded(Entity entity);
        void EntityRemoved(Entity entity);
    }

    public interface ISystemInfo
    {
        int Index { get; }
    }
}
