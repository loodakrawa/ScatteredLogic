using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ScatteredGameExample.Events;
using System;

namespace ScatteredGameExample.Systems
{
    public class InputSystem : BaseSystem
    {
        public static readonly Keys[] AllKeys = (Keys[])Enum.GetValues(typeof(Keys));
        private KeyboardState oldKeys = Keyboard.GetState();
        private MouseState oldMouse = Mouse.GetState();

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            KeyboardState newKeys = Keyboard.GetState();
            MouseState newMouse = Mouse.GetState();

            for (int i = 0; i < AllKeys.Length; ++i)
            {
                Keys key = AllKeys[i];
                CheckAndFire(oldKeys.IsKeyDown(key), newKeys.IsKeyDown(key), key);
            }

            Point pos = newMouse.Position;
            EventBus.Dispatch(new MousePositionEvent(new Vector2(pos.X, pos.Y)));

            CheckAndFire(oldMouse.LeftButton, newMouse.LeftButton, MouseButton.Left);
            CheckAndFire(oldMouse.RightButton, newMouse.RightButton, MouseButton.Right);
        }

        private void CheckAndFire(bool wasDown, bool isDown, Keys key)
        {
            if (!wasDown && isDown) EventBus.Dispatch(new KeyEvent(key, true));
            else if (wasDown && !isDown) EventBus.Dispatch(new KeyEvent(key, false));
        }

        private void CheckAndFire(bool wasDown, bool isDown, MouseButton button)
        {
            if (!wasDown && isDown) EventBus.Dispatch(new MouseButtonEvent(button, true));
            else if (wasDown && !isDown) EventBus.Dispatch(new MouseButtonEvent(button, false));
        }

        private void CheckAndFire(ButtonState previous, ButtonState current, MouseButton button)
        {
            CheckAndFire(previous == ButtonState.Pressed, current == ButtonState.Pressed, button);
        }
    }
}
