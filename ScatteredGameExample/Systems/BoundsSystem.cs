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

            transforms = EntityWorld.GetAspectComponents<Transform>(Aspect);
            colliders = EntityWorld.GetAspectComponents<Collider>(Aspect);
        }

        public override void Update(float deltaTime)
        {
            for(int i=0; i<Entities.Count; ++i)
            {
                Rectangle bounds = transforms[i].Bounds;
                Collider collider = colliders[i];

                if (collider.Group == ColliderGroup.Bullet && !this.bounds.Contains(bounds) && !this.bounds.Intersects(bounds)) EntityWorld.DestroyEntity(Entities[i]);
            }

            renderUtil.AddForDrawing(bounds);
        }
    }
}
