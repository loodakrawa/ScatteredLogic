using ScatteredGameExample.Components;
using System;

namespace ScatteredGameExample.Systems
{
    public class VelocitySystem : BaseSystem
    {
        public override Type[] RequiredComponents => RequiredTypes.From<Velocity, Transform>();

        private Transform[] transforms;
        private Velocity[] velocities;

        public override void Added()
        {
            base.Added();

            transforms = Aspect.GetComponents<Transform>();
            velocities = Aspect.GetComponents<Velocity>();
        }

        public override void Update(float deltaTime)
        {
            for (int i = 0; i < Aspect.EntityCount; ++i)
            {
                transforms[i].Position += velocities[i].Speed * deltaTime;
            }
        }
    }
}
