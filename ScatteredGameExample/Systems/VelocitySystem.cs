using ScatteredGameExample.Components;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class VelocitySystem : BaseSystem
    {
        public override IEnumerable<Type> RequiredComponents => Types.From<Velocity, Transform>();

        public override void Update(float deltaTime)
        {
            foreach(Entity entity in Entities)
            {
                Transform transform = entity.GetComponent<Transform>();
                Velocity velocity = entity.GetComponent<Velocity>();
                transform.Position += velocity.Speed * deltaTime;
            }
        }
    }
}
