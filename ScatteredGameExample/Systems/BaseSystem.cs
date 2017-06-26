﻿using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public abstract class BaseSystem : ISystem
    {
        public virtual IEnumerable<Type> RequiredComponents => Types.None;
        public IEntitySet Entities { get; set; }
        public IEntitySystemManager EntityManager { get; set; }
        public int Index { get; set; }

        public EntityFactory EntityFactory { get; set; }
        public EventBus EventBus { get; set; }


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
