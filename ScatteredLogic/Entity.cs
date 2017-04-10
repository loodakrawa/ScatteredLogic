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

        public string Name
        {
            get { return entityManager.GetName(this); }
            set { entityManager.SetName(this, value); }
        }

        private readonly IEntityManager<Entity> entityManager;

        public Entity(IEntityManager<Entity> entityManager, int id)
        {
            Id = id;
            this.entityManager = entityManager;
        }

        public void AddTag(string tag) => entityManager.AddTag(this, tag);
        public void RemoveTag(string tag) => entityManager.RemoveTag(this, tag);
        public SetEnumerable<string> GetTags() => entityManager.GetEntityTags(this);

        public void AddComponent<T>(T component) => entityManager.AddComponent(this, component);
        public void AddComponent(object component, Type type) => entityManager.AddComponent(this, component, type);
        public void RemoveComponent<T>() => entityManager.RemoveComponent<T>(this);
        public void RemoveComponent(Type type) => entityManager.RemoveComponent(this, type);
        public void RemoveComponent(object component) => entityManager.RemoveComponent(this, component);
        public bool HasComponent<T>() => entityManager.HasComponent<T>(this);
        public bool HasComponent(Type type) => entityManager.HasComponent(this, type);
        public T GetComponent<T>() => entityManager.GetComponent<T>(this);
        public T GetComponent<T>(Type type) => entityManager.GetComponent<T>(this, type);
        public void Destroy() => entityManager.DestroyEntity(this);

        public bool Equals(Entity other) => Id == other.Id;
        public override bool Equals(object obj) => obj is Entity ? Equals((Entity)obj) : false;

        public override int GetHashCode() => Id;

        public override string ToString() => Id.ToString();

        public static bool operator ==(Entity a, Entity b) => a.Id == b.Id;
        public static bool operator !=(Entity a, Entity b) => a.Id != b.Id;
    }
}
