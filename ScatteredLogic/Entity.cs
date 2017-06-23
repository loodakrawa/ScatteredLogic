// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;

namespace ScatteredLogic
{
    public struct Entity : IEquatable<Entity>
    {
        public readonly int Id;
        internal readonly int Version;

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

        public T GetComponent<T>() => entityManager.GetComponent<T>(this);
        public object GetComponent(Type type) => entityManager.GetComponent(this, type);

        public bool Equals(Entity other) => Id == other.Id && Version == other.Version;
        public override bool Equals(object obj) => obj is Entity ? Equals((Entity)obj) : false;

        public override int GetHashCode() => unchecked((92821 + Id.GetHashCode()) * 92821 + Version.GetHashCode());

        public override string ToString() => string.Format("{0}|{1}", Id, Version);

        public static bool operator ==(Entity a, Entity b) => a.Id == b.Id;
        public static bool operator !=(Entity a, Entity b) => a.Id != b.Id;
    }
}
