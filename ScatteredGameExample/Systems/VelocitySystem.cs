using ScatteredGameExample.Components;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class VelocitySystem : BaseSystem
    {
        public override IEnumerable<Type> RequiredComponents => Types.From<Velocity, Transform>();

        private Transform[] transforms;
        private Velocity[] velocities;

        public override void Added()
        {
            base.Added();

            transforms = EntityManager.GetComponents<Transform>();
            velocities = EntityManager.GetComponents<Velocity>();
        }

        public override void Update(float deltaTime)
        {
            foreach(Entity entity in Entities)
            {
                transforms[entity.Id].Position += velocities[entity.Id].Speed * deltaTime;
            }
        }
    }
}
