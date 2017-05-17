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

                spriteBatch.Draw(texture, pos, Color.White);
            }
        }
    }
}
