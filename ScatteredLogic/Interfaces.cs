// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;

namespace ScatteredLogic
{
    public interface IArray<T>
    {
        int Count { get; }
        T this[int i] { get; }
    }

    public interface IAspect
    {
        IArray<Handle> Entities { get; }
        IArray<T> GetComponents<T>();
    }

    public interface IEntityWorld
    {
        IArray<Handle> Entities { get; }

        Handle CreateEntity();
        void DestroyEntity(Handle handle);
        bool ContainsEntity(Handle handle);

        void AddComponent<T>(Handle handle, T component);
        void RemoveComponent<T>(Handle handle);
        T GetComponent<T>(Handle handle);

        void Commit();

        IAspect CreateAspect(Type[] types);
    }
}
