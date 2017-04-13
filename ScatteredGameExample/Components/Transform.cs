using Microsoft.Xna.Framework;

namespace ScatteredGameExample.Components
{
    public class Transform
    {
        private Rectangle bounds;

        public Vector2 Position { get; set; }
        public Vector2 Scale { get; set; }
        //public Rectangle Bounds
        //{
        //    get { return bounds; }
        //    set { bounds = new Rectangle(value)}
        //}
    }
}
