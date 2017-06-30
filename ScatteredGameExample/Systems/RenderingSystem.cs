using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ScatteredGameExample.Components;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class RenderingSystem : BaseSystem, DrawingSystem
    {
        public override IEnumerable<Type> RequiredComponents => RequiredTypes.From<TextureHandle, Transform>();

        private readonly ContentManager contentManager;
        private readonly List<Texture2D> textures = new List<Texture2D>();
        private readonly List<string> paths = new List<string>();

        private IArray<TextureHandle> textureHandles;
        private IArray<Transform> transforms;

        public RenderingSystem(ContentManager contentManager)
        {
            this.contentManager = contentManager;
        }

        public TextureHandle Load(string path)
        {
            int index = paths.IndexOf(path);
            if (index < 0)
            {
                Texture2D tex = contentManager.Load<Texture2D>(path);
                index = textures.Count;
                textures.Add(tex);
                paths.Add(path);
            }
            return new TextureHandle(index);
        }

        public override void Added()
        {
            base.Added();

            textureHandles = EntityWorld.GetComponents<TextureHandle>();
            transforms = EntityWorld.GetComponents<Transform>();
        }

        public void Draw(float deltaTime, SpriteBatch spriteBatch)
        {
            IArray<Handle> entities = EntityWorld.GetEntitiesForGroup(GroupId);

            foreach (Handle entity in entities)
            {
                TextureHandle th = textureHandles[entity.Index];
                Transform tr = transforms[entity.Index];

                Texture2D tx = textures[th.Id];
                Vector2 scale = tr.Size / tx.Bounds.Size.ToVector2();
                Vector2 origin = new Vector2(0.5f * tx.Width, 0.5f * tx.Height);

                spriteBatch.Draw(tx, tr.Position, null, Color.White, tr.Rotation, origin, scale, SpriteEffects.None, 0.5f);
            }
        }
    }
}
