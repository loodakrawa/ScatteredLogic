using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ScatteredGameExample.Events
{
    public class KeyEvent
    {
        public Keys Key { get; set; }
        public bool Pressed { get; set; }
    }

    public class MouseButtonEvent
    {
        public MouseButton Button { get; set; }
        public bool Pressed { get; set; }
    }

    public class MousePositionEvent
    {
        public Vector2 Position { get; set; }
    }
}
