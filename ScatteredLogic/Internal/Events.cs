// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

namespace ScatteredLogic.Internal
{
    internal struct DeleteEntityEvent
    {
        public readonly Entity Entity;

        public DeleteEntityEvent(Entity entity)
        {
            Entity = entity;
        }
    }

    internal struct RemoveComponentEvent<T>
    {
        public readonly Entity Entity;

        public RemoveComponentEvent(Entity entity)
        {
            Entity = entity;
        }
    }

    internal struct AddComponentEvent<T>
    {
        public readonly Entity Entity;
        public readonly T Component;

        public AddComponentEvent(Entity entity, T component)
        {
            Entity = entity;
            Component = component;
        }
    }
}
