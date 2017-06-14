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

        private readonly MousePositionEvent mpe = new MousePositionEvent();
        private readonly GroupObjectPool<MouseButtonEvent> mouseButtonPool = new GroupObjectPool<MouseButtonEvent>();
        private readonly GroupObjectPool<KeyEvent> keyEventPool = new GroupObjectPool<KeyEvent>();

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            mouseButtonPool.ReturnAll();
            keyEventPool.ReturnAll();

            KeyboardState newKeys = Keyboard.GetState();
            MouseState newMouse = Mouse.GetState();

            for (int i = 0; i < AllKeys.Length; ++i)
            {
                Keys key = AllKeys[i];
                CheckAndFire(oldKeys.IsKeyDown(key), newKeys.IsKeyDown(key), key);
            }

            Point pos = newMouse.Position;
            mpe.Position = new Vector2(pos.X, pos.Y);
            for (int i=0; i<10000; ++i) EventBus.Dispatch(mpe);

            CheckAndFire(oldMouse.LeftButton, newMouse.LeftButton, MouseButton.Left);
            CheckAndFire(oldMouse.RightButton, newMouse.RightButton, MouseButton.Right);

            oldKeys = newKeys;
            oldMouse = newMouse;
        }

        private void CheckAndFire(bool wasDown, bool isDown, Keys key)
        {
            if (!wasDown && isDown) FireKey(key, true);
            else if (wasDown && !isDown) FireKey(key, false);
        }

        private void FireKey(Keys key, bool pressed)
        {
            KeyEvent keyEvent = keyEventPool.Get();
            keyEvent.Key = key;
            keyEvent.Pressed = pressed;
            EventBus.Dispatch(keyEvent);
        }

        private void CheckAndFire(bool wasDown, bool isDown, MouseButton button)
        {
            if (!wasDown && isDown) FireMouseButton(button, true);
            else if (wasDown && !isDown) FireMouseButton(button, false);
        }

        private void CheckAndFire(ButtonState previous, ButtonState current, MouseButton button)
        {
            CheckAndFire(previous == ButtonState.Pressed, current == ButtonState.Pressed, button);
        }

        private void FireMouseButton(MouseButton button, bool pressed)
        {
            MouseButtonEvent mbEvent = mouseButtonPool.Get();
            mbEvent.Button = button;
            mbEvent.Pressed = pressed;
            EventBus.Dispatch(mbEvent);
        }
    }
}
