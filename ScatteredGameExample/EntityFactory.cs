using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ScatteredGameExample.Components;
using ScatteredLogic;

namespace ScatteredGameExample
{
    public class EntityFactory
    {
        private const string TextureCrosshair = "crosshair";
        private const string TextureSquare = "square";

        private readonly ContentManager content;
        private readonly IEntityManager<Entity> entityManager;

        public EntityFactory(ContentManager content, IEntityManager<Entity> entityManager)
        {
            this.content = content;
            this.entityManager = entityManager;
        }

        public Entity CreateCrosshair()
        {
            Entity e = entityManager.CreateEntity();
            e.Name = "Crosshair";
            e.AddComponent(new Transform { Size = new Vector2(50, 50) });
            e.AddComponent(content.Load<Texture2D>(TextureCrosshair));
            return e;
        }

        public Entity CreateSquare()
        {
            Entity e = entityManager.CreateEntity();
            e.AddComponent(content.Load<Texture2D>(TextureSquare));
            e.AddComponent(new Transform());
            return e;
        }

        public Entity CreateBullet()
        {
            Entity bullet = CreateSquare();
            bullet.AddComponent(new Transform { Size = new Vector2(5, 5) });
            bullet.AddComponent(new Collider { Group = ColliderGroup.Bullet });
            bullet.AddComponent(new Velocity());
            return bullet;
        }
    }
}
