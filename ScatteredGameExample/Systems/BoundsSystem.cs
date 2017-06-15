using Microsoft.Xna.Framework;
using ScatteredGameExample.Components;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class BoundsSystem : BaseSystem
    {
        private readonly Rectangle bounds;
        private readonly RenderUtil renderUtil;

        public override IEnumerable<Type> RequiredComponents => Types.From<Transform, Collider>();

        public BoundsSystem(int x, int y, int width, int height, RenderUtil renderUtil)
        {
            bounds = new Rectangle(x, y, width, height);
            this.renderUtil = renderUtil;
        }

        public override void Update(float deltaTime)
        {
            foreach(Entity entity in Entities)
            {
                Transform transform = entity.GetComponent<Transform>();
                Rectangle bounds = transform.Bounds;
                Collider collider = entity.GetComponent<Collider>();

                if (collider.Group == ColliderGroup.Bullet && !this.bounds.Contains(bounds) && !this.bounds.Intersects(bounds)) entity.Destroy();
            }

            renderUtil.AddForDrawing(bounds);
        }
    }
}
