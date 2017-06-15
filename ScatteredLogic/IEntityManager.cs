// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic
{
    public interface IEntityManager<E> where E : struct, IEquatable<E>
    {
        EventBus EventBus { get; }

        E CreateEntity();
        void DestroyEntity(E entity);
        bool ContainsEntity(E entity);

        void AddComponent<T>(E entity, T component);
        void AddComponent(E entity, object component, Type type);
        void RemoveComponent<T>(E entity);
        void RemoveComponent(E entity, Type type);
        void RemoveComponent(E entity, object component);
        bool HasComponent<T>(E entity);
        bool HasComponent(E entity, Type type);
        T GetComponent<T>(E entity);
        T GetComponent<T>(E entity, Type type);

        void AddSystem(ISystem<E> system);
        void RemoveSystem(ISystem<E> system);

        string GetName(E entity);
        void SetName(E entity, string name);

        void AddTag(E entity, string tag);
        void RemoveTag(E entity, string tag);
        bool HasTag(E entity, string tag);
        SetEnumerable<string> GetEntityTags(E entity);
        SetEnumerable<E> GetEntitiesWithTag(string tag);

        E? FindEntity(Func<E, bool> predicate);
        void FindEntities(Func<E, bool> predicate, ICollection<E> results);
        SetEnumerable<E> GetAllEntities();

        void Update(float deltaTime);
    }
}
