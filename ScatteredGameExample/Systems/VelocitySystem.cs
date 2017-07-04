using ScatteredGameExample.Components;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class VelocitySystem : BaseSystem
    {
        public override IEnumerable<Type> RequiredComponents => RequiredTypes.From<Velocity, Transform>();

        private IArray<Transform> transforms;
        private IArray<Velocity> velocities;

        public override void Added()
        {
            base.Added();

            transforms = EntityWorld.GetAspectComponents<Transform>(Aspect);
            velocities = EntityWorld.GetAspectComponents<Velocity>(Aspect);
        }

        public override void Update(float deltaTime)
        {
            for(int i=0; i<Entities.Count; ++i)
            {
                transforms[i].Position += velocities[i].Speed * deltaTime;
            }
        }
    }
}
