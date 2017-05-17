using Microsoft.Xna.Framework;

namespace ScatteredGameExample.Components
{
    public class Transform
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Rectangle Bounds => new Rectangle(Position.ToPoint(), Size.ToPoint());
    }
}
