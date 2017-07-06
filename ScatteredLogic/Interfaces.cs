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
        void DestroyEntity(Handle handle);
        bool ContainsEntity(Handle handle);

        void AddComponent<T>(Handle handle, T component);
        void RemoveComponent<T>(Handle handle);
        T GetComponent<T>(Handle handle);

        Handle CreateAspect(IEnumerable<Type> types, string name);
        IArray<T> GetAspectComponents<T>(Handle handle);
        IArray<Handle> GetAspectEntities(Handle handle);
        void UpdateAspect(Handle handle);

        void Step();
    }
}
