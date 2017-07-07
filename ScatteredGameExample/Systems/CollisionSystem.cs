using ScatteredGameExample.Components;
using ScatteredGameExample.Events;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class CollisionSystem : BaseSystem
    {
        public override Type[] RequiredComponents => RequiredTypes.From<Transform, Collider>();

        private List<Handle> entities = new List<Handle>();

        public override void EntityAdded(Handle entity) => entities.Add(entity);
        public override void EntityRemoved(Handle entity) => entities.Remove(entity);

        private readonly GroupObjectPool<CollisionEvent> eventPool = new GroupObjectPool<CollisionEvent>();

        public override void Update(float deltaTime)
        {
            eventPool.ReturnAll();

            for (int i = 0; i < entities.Count; ++i)
            {
                Handle e1 = entities[i];
                Transform e1t = EntityWorld.GetComponent<Transform>(e1);
                for (int j = i + 1; j < entities.Count; ++j)
                {
                    Handle e2 = entities[j];
                    Transform e2t = EntityWorld.GetComponent<Transform>(e2);
                    if (e1t.Bounds.Intersects(e2t.Bounds))
                    {
                        CollisionEvent e = eventPool.Get();
                        e.First = e1;
                        e.Second = e2;
                        EventBus.Dispatch(e);
                    }
                }
            }
        }
    }
}
