using ScatteredGameExample.Components;
using ScatteredGameExample.Events;
using ScatteredLogic;
using System;

namespace ScatteredGameExample.Systems
{
    public class CollisionSystem : BaseSystem
    {
        public override Type[] RequiredComponents => RequiredTypes.From<Transform, Collider>();

        private readonly GroupObjectPool<CollisionEvent> eventPool = new GroupObjectPool<CollisionEvent>();

        public override void Update(float deltaTime)
        {
            eventPool.ReturnAll();

            Entity[] entities = Aspect.Entities;
            int count = Aspect.EntityCount;

            for (int i = 0; i < count; ++i)
            {
                Entity e1 = entities[i];
                Transform e1t = EntityWorld.GetComponent<Transform>(e1);
                for (int j = i + 1; j < count; ++j)
                {
                    Entity e2 = entities[j];
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
