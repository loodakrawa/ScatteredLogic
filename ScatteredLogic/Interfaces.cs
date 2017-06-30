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
        int Count { get; }
        T this[int i] { get; }
        ArrayEnumerator<T> GetEnumerator();
    }

    public interface IEntityWorld
    {
        IArray<Handle> Entities { get; }

        Handle CreateEntity();
        void DestroyEntity(Handle entity);
        bool ContainsEntity(Handle entity);

        void AddComponent<T>(Handle entity, T component);
        void RemoveComponent<T>(Handle entity);
        T GetComponent<T>(Handle entity);

        IArray<T> GetComponents<T>();
    }

    public interface IGroupedEntityWorld : IEntityWorld
    {
        int GetGroupId(IEnumerable<Type> types);
        IArray<Handle> GetEntitiesForGroup(int groupId);
    }

}
