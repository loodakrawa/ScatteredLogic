using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScatteredGameExample.Components;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class RenderingSystem : BaseSystem, DrawingSystem
    {
        public override IEnumerable<Type> RequiredComponents => Types.From<Texture2D, Transform>();

        public void Draw(float deltaTime, SpriteBatch spriteBatch)
        {
            foreach(Entity entity in Entities)
            {
                Texture2D texture = entity.GetComponent<Texture2D>();
                Transform transform = entity.GetComponent<Transform>();

                Vector2 pos = transform.Position - transform.Size / 2;
                Vector2 scale = transform.Size / texture.Bounds.Size.ToVector2();

                spriteBatch.Draw(texture, pos, null, Color.White, 0, Vector2.Zero, scale.X, SpriteEffects.None, 0.5f);
            }
        }
    }
}
