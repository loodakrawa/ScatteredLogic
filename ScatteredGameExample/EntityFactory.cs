using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ScatteredGameExample.Components;
using ScatteredGameExample.Systems;
using ScatteredLogic;

namespace ScatteredGameExample
{
    public class EntityFactory
    {
        private const string TextureCrosshair = "crosshair";
        private const string TextureSquare = "square";

        private readonly RenderingSystem renderingSystem;
        private readonly IEntityWorld entityManager;

        public EntityFactory(RenderingSystem renderingSystem, IEntityWorld entityManager)
        {
            this.renderingSystem = renderingSystem;
            this.entityManager = entityManager;
        }

        public Handle CreateCrosshair()
        {
            Handle e = entityManager.CreateEntity();
            entityManager.AddComponent(e, new Transform { Size = new Vector2(50, 50) });
            entityManager.AddComponent(e, renderingSystem.Load(TextureCrosshair));
            return e;
        }

        public Handle CreateSquare()
        {
            Handle e = entityManager.CreateEntity();
            entityManager.AddComponent(e, renderingSystem.Load(TextureSquare));
            entityManager.AddComponent(e, new Transform());
            return e;
        }

        public Handle CreateBullet(float rotation, Vector2 position, Velocity velocity)
        {
            Handle bullet = CreateSquare();
            entityManager.AddComponent(bullet, new Transform { Size = new Vector2(5, 5), Position = position, Rotation = rotation });
            entityManager.AddComponent(bullet, new Collider { Group = ColliderGroup.Bullet });
            entityManager.AddComponent(bullet, velocity);
            return bullet;
        }
    }
}
