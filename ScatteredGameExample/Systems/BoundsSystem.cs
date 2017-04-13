using Microsoft.Xna.Framework;
using ScatteredGameExample.Components;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class BoundsSystem : BaseSystem
    {
        private Rectangle size;

        public override IEnumerable<Type> RequiredComponents => Types.From<Velocity, Transform>();

        public BoundsSystem(int width, int height)
        {
            size = new Rectangle(-width / 2, -height / 2, width, height);
        }

        public override void Update(float deltaTime)
        {
            foreach(Entity entity in Entities)
            {
                Transform transform = entity.GetComponent<Transform>();
                //if(size.Intersects(transform.Position))
            }
        }
    }
}
