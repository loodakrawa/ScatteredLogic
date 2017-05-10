using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ScatteredGameExample.Events
{
    public struct KeyEvent
    {
        public readonly Keys Key;
        public readonly bool Pressed;

        public KeyEvent(Keys key, bool pressed)
        {
            this.Key = key;
            this.Pressed = pressed;
        }
    }

    public struct MouseButtonEvent
    {
        public readonly MouseButton Button;
        public readonly bool Pressed;

        public MouseButtonEvent(MouseButton button, bool pressed)
        {
            this.Button = button;
            this.Pressed = pressed;
        }
    }

    public struct MousePositionEvent
    {
        public readonly Vector2 Position;
        
        public MousePositionEvent(Vector2 position)
        {
            this.Position = position;
        }
    }
}
