using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ScatteredGameExample.Components;
using ScatteredGameExample.Events;
using ScatteredLogic;
using System;

namespace ScatteredGameExample.Systems
{
    public class PlayerControllerSystem : BaseSystem
    {
        private static readonly float MaxPlayerSpeed = 100;
        private static readonly float BulletSpeed = 200;

        private Handle player;
        private Vector2 mouseLocation;

        public override void Added()
        {
            base.Added();

            player = EntityFactory.CreateSquare();
            EntityWorld.AddComponent(player, new Transform { Size = new Vector2(20, 20) });
            EntityWorld.AddComponent(player, new Velocity());

            EventBus.Register<KeyEvent>(OnKey);
            EventBus.Register<MousePositionEvent>(OnMousePosition);
            EventBus.Register<MouseButtonEvent>(OnMouseButton);

            InputSystem.RegisterKey(Keys.Up);
            InputSystem.RegisterKey(Keys.Down);
            InputSystem.RegisterKey(Keys.Left);
            InputSystem.RegisterKey(Keys.Right);
        }

        private void OnMouseButton(MouseButtonEvent e)
        {
            if (!e.Pressed) return;

            Transform pTransform = EntityWorld.GetComponent<Transform>(player);
            Velocity velocity = new Velocity();
            velocity.Speed = Vector2.Transform(new Vector2(BulletSpeed, 0), Matrix.CreateRotationZ(pTransform.Rotation));

            Handle bullet = EntityFactory.CreateBullet(pTransform.Rotation, pTransform.Position, velocity);

            Transform bTransform = EntityWorld.GetComponent<Transform>(bullet);

        }

        private void OnMousePosition(MousePositionEvent e)
        {
            mouseLocation = e.Position;
            Transform transform = EntityWorld.GetComponent<Transform>(player);
            if (transform != null)
            {
                Vector2 pPos = transform.Position;
                transform.Rotation = (float)Math.Atan2(mouseLocation.Y - pPos.Y, mouseLocation.X - pPos.X);
            }
        }

        private void OnKey(KeyEvent e)
        {
            Velocity velocity = EntityWorld.GetComponent<Velocity>(player);
            float x = velocity.Speed.X;
            float y = velocity.Speed.Y;

            switch (e.Key)
            {
                case Keys.Up:
                    y += e.Pressed ? -MaxPlayerSpeed : MaxPlayerSpeed;
                    break;
                case Keys.Down:
                    y += e.Pressed ? MaxPlayerSpeed : -MaxPlayerSpeed;
                    break;
                case Keys.Left:
                    x += e.Pressed ? -MaxPlayerSpeed : MaxPlayerSpeed;
                    break;
                case Keys.Right:
                    x += e.Pressed ? MaxPlayerSpeed : -MaxPlayerSpeed;
                    break;
                default:
                    break;
            }

            if (Math.Abs(x) >= MaxPlayerSpeed) x = Math.Sign(x) * MaxPlayerSpeed;
            if (Math.Abs(y) >= MaxPlayerSpeed) y = Math.Sign(y) * MaxPlayerSpeed;

            velocity.Speed = new Vector2(x, y);
        }
    }
}
