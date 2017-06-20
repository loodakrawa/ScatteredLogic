﻿using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public abstract class BaseSystem : ISystem
    {
        public IEntitySet Entities { get; set; }

        public IEntityManager EntityManager { get; set; }

        public EventBus EventBus { get; set; }

        public EntityFactory EntityFactory { get; set; }

        public virtual IEnumerable<Type> RequiredComponents => Types.None;

        public virtual void Added()
        {
        }

        public virtual void EntityAdded(Entity entity)
        {
        }

        public virtual void EntityRemoved(Entity entity)
        {
        }

        public virtual void Removed()
        {
        }

        public virtual void Update(float deltaTime)
        {
        }
    }
}
