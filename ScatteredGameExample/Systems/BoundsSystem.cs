using Microsoft.Xna.Framework;
using ScatteredGameExample.Components;
using ScatteredLogic;
using System;

namespace ScatteredGameExample.Systems
{
    public class BoundsSystem : BaseSystem
    {
        public override Type[] RequiredComponents => RequiredTypes.From<Transform, Collider>();

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

            transforms = Aspect.GetComponents<Transform>();
            colliders = Aspect.GetComponents<Collider>();
        }

        public override void Update(float deltaTime)
        {
            for (int i = 0; i < Aspect.Entities.Count; ++i)
            {
                Rectangle bounds = transforms[i].Bounds;
                Collider collider = colliders[i];

                if (collider.Group == ColliderGroup.Bullet && !this.bounds.Contains(bounds) && !this.bounds.Intersects(bounds)) EntityWorld.DestroyEntity(Aspect.Entities[i]);
            }

            renderUtil.AddForDrawing(bounds);
        }
    }
}
