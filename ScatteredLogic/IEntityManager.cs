﻿// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace ScatteredLogic
{
    public interface IEntityManager
    {
        EventBus EventBus { get; }

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

        void AddSystem(ISystem system);
        void RemoveSystem(ISystem system);

        void Update(float deltaTime);
    }
}
