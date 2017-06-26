using ScatteredGameExample.Components;
using ScatteredGameExample.Events;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class CollisionSystem : BaseSystem
    {
        public override IEnumerable<Type> RequiredComponents => RequiredTypes.From<Transform, Collider>();

        private List<Entity> entities = new List<Entity>();

        public override void EntityAdded(Entity entity) => entities.Add(entity);
        public override void EntityRemoved(Entity entity) => entities.Remove(entity);

        private readonly GroupObjectPool<CollisionEvent> eventPool = new GroupObjectPool<CollisionEvent>();

        public override void Update(float deltaTime)
        {
            eventPool.ReturnAll();

            for (int i = 0; i < entities.Count; ++i)
            {
                Entity e1 = entities[i];
                Transform e1t = e1.GetComponent<Transform>();
                for (int j = i + 1; j < entities.Count; ++j)
                {
                    Entity e2 = entities[j];
                    Transform e2t = e2.GetComponent<Transform>();
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
