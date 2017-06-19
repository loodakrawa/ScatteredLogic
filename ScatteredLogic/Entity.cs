// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;

namespace ScatteredLogic
{
    public struct Entity : IEquatable<Entity>
    {
        public readonly int Id;
        public readonly int Version;

        private readonly IEntityManager entityManager;

        internal Entity(IEntityManager entityManager, int id, int version)
        {
            Id = id;
            Version = version;
            this.entityManager = entityManager;
        }

        public void Destroy() => entityManager.DestroyEntity(this);

        public void AddComponent<T>(T component) => entityManager.AddComponent(this, component);
        public void AddComponent(object component, Type type) => entityManager.AddComponent(this, component, type);

        public void RemoveComponent<T>() => entityManager.RemoveComponent<T>(this);
        public void RemoveComponent(Type type) => entityManager.RemoveComponent(this, type);
        public void RemoveComponent(object component) => entityManager.RemoveComponent(this, component);
        public void RemoveComponent(int typeId) => entityManager.RemoveComponent(this, typeId);

        public bool HasComponent<T>() => entityManager.HasComponent<T>(this);
        public bool HasComponent(Type type) => entityManager.HasComponent(this, type);
        public bool HasComponent(int typeId) => entityManager.HasComponent(this, typeId);

        public T GetComponent<T>() => entityManager.GetComponent<T>(this);
        public object GetComponent(Type type) => entityManager.GetComponent(this, type);
        public T GetComponent<T>(int typeId) => entityManager.GetComponent<T>(this, typeId);

        public bool Equals(Entity other) => Id == other.Id && Version == other.Version;
        public override bool Equals(object obj) => obj is Entity ? Equals((Entity)obj) : false;

        public override int GetHashCode() => Id;

        public override string ToString() => Id.ToString();

        public static bool operator ==(Entity a, Entity b) => a.Id == b.Id;
        public static bool operator !=(Entity a, Entity b) => a.Id != b.Id;
    }
}
