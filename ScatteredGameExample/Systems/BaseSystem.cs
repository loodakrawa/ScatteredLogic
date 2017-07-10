using ScatteredLogic;
using System;

namespace ScatteredGameExample.Systems
{
    public abstract class BaseSystem
    {
        public virtual Type[] RequiredComponents => RequiredTypes.None;
        public IAspect Aspect { get; set; }
        public IEntityWorld EntityWorld { get; set; }

        public EntityFactory EntityFactory { get; set; }
        public EventBus EventBus { get; set; }
        public InputSystem InputSystem { get; set; }

        public virtual void Added() { }

        public virtual void EntityAdded(Handle entity) { }
        public virtual void EntityRemoved(Handle entity) { }

        public virtual void Update(float deltaTime) { }
    }
}
