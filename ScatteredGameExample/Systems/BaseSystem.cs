using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public abstract class BaseSystem
    {
        public virtual IEnumerable<Type> RequiredComponents => RequiredTypes.None;
        public Handle Aspect { get; set; }
        public IArray<Handle> Entities { get; set; }
        public IEntityWorld EntityWorld { get; set; }

        public EntityFactory EntityFactory { get; set; }
        public EventBus EventBus { get; set; }

        public virtual void Added() { }

        public virtual void EntityAdded(Handle entity) { }
        public virtual void EntityRemoved(Handle entity) { }

        public virtual void Update(float deltaTime) { }
    }
}
