using Microsoft.Xna.Framework.Graphics;

namespace ScatteredGameExample.Systems
{
    interface DrawingSystem
    {
        void Draw(float deltaTime, SpriteBatch spriteBatch);
    }
}
