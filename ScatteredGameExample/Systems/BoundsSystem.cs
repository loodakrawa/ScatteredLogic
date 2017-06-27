using Microsoft.Xna.Framework;
using ScatteredGameExample.Components;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class BoundsSystem : BaseSystem
    {
        public override IEnumerable<Type> RequiredComponents => RequiredTypes.From<Transform, Collider>();

        private readonly Rectangle bounds;
        private readonly RenderUtil renderUtil;

        private IArray<Transform> transforms;
        private IArray<Collider> colliders;

        public BoundsSystem(int x, int y, int width, int height, RenderUtil renderUtil)
        {
            bounds = new Rectangle(x, y, width, height);
            this.renderUtil = renderUtil;
        }

        public override void Added()
        {
            base.Added();

            transforms = EntityWorld.GetComponents<Transform>();
            colliders = EntityWorld.GetComponents<Collider>();
        }

        public override void Update(float deltaTime)
        {
            foreach(Entity entity in Entities)
            {
                Rectangle bounds = transforms[entity.Id].Bounds;
                Collider collider = colliders[entity.Id];

                if (collider.Group == ColliderGroup.Bullet && !this.bounds.Contains(bounds) && !this.bounds.Intersects(bounds)) entity.Destroy();
            }

            renderUtil.AddForDrawing(bounds);
        }
    }
}
