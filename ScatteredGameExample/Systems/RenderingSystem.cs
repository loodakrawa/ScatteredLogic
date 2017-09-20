using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ScatteredGameExample.Components;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class RenderingSystem : BaseSystem
    {
        public override Type[] RequiredComponents => RequiredTypes.From<TextureHandle, Transform>();

        private readonly ContentManager contentManager;
        private readonly List<Texture2D> textures = new List<Texture2D>();
        private readonly List<string> paths = new List<string>();

        private TextureHandle[] textureHandles;
        private Transform[] transforms;

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

            textureHandles = Aspect.GetComponents<TextureHandle>();
            transforms = Aspect.GetComponents<Transform>();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Aspect.EntityCount; ++i)
            {
                TextureHandle th = textureHandles[i];
                Transform tr = transforms[i];

                Texture2D tx = textures[th.Id];
                Vector2 scale = tr.Size / tx.Bounds.Size.ToVector2();
                Vector2 origin = new Vector2(0.5f * tx.Width, 0.5f * tx.Height);

                spriteBatch.Draw(tx, tr.Position, null, Color.White, tr.Rotation, origin, scale, SpriteEffects.None, 0.5f);
            }
        }
    }
}
