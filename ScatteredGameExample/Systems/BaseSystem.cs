using Microsoft.Xna.Framework.Graphics;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public abstract class BaseSystem : ISystem<Entity>
    {
        public SetEnumerable<Entity> Entities { get; set; }

        public IEntityManager<Entity> EntityManager { get; set; }

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
