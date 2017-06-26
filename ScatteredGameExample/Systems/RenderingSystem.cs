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
                Texture2D tx = entity.GetComponent<Texture2D>();
                Transform tr = entity.GetComponent<Transform>();

                Vector2 scale = tr.Size / tx.Bounds.Size.ToVector2();
                Vector2 origin = new Vector2(0.5f * tx.Width, 0.5f * tx.Height);

                spriteBatch.Draw(tx, tr.Position, null, Color.White, tr.Rotation, origin, scale, SpriteEffects.None, 0.5f);
            }
        }
    }
}
