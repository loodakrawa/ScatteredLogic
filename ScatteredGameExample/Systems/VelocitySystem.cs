﻿using ScatteredGameExample.Components;
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

            transforms = EntityWorld.GetComponents<Transform>();
            velocities = EntityWorld.GetComponents<Velocity>();
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
