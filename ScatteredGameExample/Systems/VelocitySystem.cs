using ScatteredGameExample.Components;
using ScatteredLogic;
using System;

namespace ScatteredGameExample.Systems
{
    public class VelocitySystem : BaseSystem
    {
        public override Type[] RequiredComponents => RequiredTypes.From<Velocity, Transform>();

        private IArray<Transform> transforms;
        private IArray<Velocity> velocities;

        public override void Added()
        {
            base.Added();

            transforms = Aspect.GetComponents<Transform>();
            velocities = Aspect.GetComponents<Velocity>();
        }

        public override void Update(float deltaTime)
        {
            for (int i = 0; i < Aspect.Entities.Count; ++i)
            {
                transforms[i].Position += velocities[i].Speed * deltaTime;
            }
        }
    }
}
