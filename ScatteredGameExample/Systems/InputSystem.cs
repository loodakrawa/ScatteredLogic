using Microsoft.Xna.Framework.Input;
using ScatteredGameExample.Events;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample.Systems
{
    public class InputSystem
    {
        private readonly EventBus eventBus;

        public readonly HashSet<Keys> registeredKeys = new HashSet<Keys>();
        private KeyboardState oldKeys = Keyboard.GetState();
        private MouseState oldMouse = Mouse.GetState();

        public InputSystem(EventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public void Update()
        {
            KeyboardState newKeys = Keyboard.GetState();
            MouseState newMouse = Mouse.GetState();

            foreach (Keys key in registeredKeys) CheckAndFire(oldKeys.IsKeyDown(key), newKeys.IsKeyDown(key), key);

            eventBus.Dispatch(new MousePositionEvent(newMouse.Position.ToVector2()));

            CheckAndFire(oldMouse.LeftButton, newMouse.LeftButton, MouseButton.Left);
            CheckAndFire(oldMouse.RightButton, newMouse.RightButton, MouseButton.Right);

            oldKeys = newKeys;
            oldMouse = newMouse;
        }

        public void RegisterKey(Keys key) => registeredKeys.Add(key);

        private void CheckAndFire(bool wasDown, bool isDown, Keys key)
        {
            if (!wasDown && isDown) FireKey(key, true);
            else if (wasDown && !isDown) FireKey(key, false);
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

        private void FireMouseButton(MouseButton button, bool pressed) => eventBus.Dispatch(new MouseButtonEvent(button, pressed));
        private void FireKey(Keys key, bool pressed) => eventBus.Dispatch(new KeyEvent(key, pressed));
    }
}
