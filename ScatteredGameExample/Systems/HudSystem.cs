using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScatteredGameExample.Events;

namespace ScatteredGameExample.Systems
{
    public class HudSystem : BaseSystem, DrawingSystem
    {
        private readonly Texture2D crosshair;

        private Vector2 mousePos;
        private Vector2 mouseOffset;

        public HudSystem(Texture2D crosshair)
        {
            this.crosshair = crosshair;
            mouseOffset = new Vector2(-crosshair.Width / 2, -crosshair.Height / 2);
        }

        public override void Added()
        {
            base.Added();
            EventBus.Register<MousePositionEvent>(e => mousePos = e.Position);
        }

        public void Draw(float deltaTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(crosshair, mousePos + mouseOffset, Color.White);
        }
    }
}
