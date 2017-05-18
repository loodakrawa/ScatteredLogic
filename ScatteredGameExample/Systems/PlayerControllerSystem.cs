using System;
using Microsoft.Xna.Framework;
using ScatteredGameExample.Components;
using ScatteredGameExample.Events;
using ScatteredLogic;
using Microsoft.Xna.Framework.Input;

namespace ScatteredGameExample.Systems
{
    public class PlayerControllerSystem : BaseSystem
    {
        private static readonly float Velocity = 100;

        private Entity player;

        public override void Added()
        {
            base.Added();

            player = EntityFactory.CreateSquare();
            player.GetComponent<Transform>().Size = new Vector2(20, 20);
            player.AddComponent(new Velocity());

            EventBus.Register<KeyEvent>(OnKey);
        }

        private void OnKey(KeyEvent e)
        {
            Velocity velocity = player.GetComponent<Velocity>();
            float x = velocity.Speed.X;
            float y = velocity.Speed.Y;

            switch (e.Key)
            {
                case Keys.Up:
                    y += e.Pressed ? -Velocity : Velocity;
                    break;
                case Keys.Down:
                    y += e.Pressed ? Velocity : -Velocity;
                    break;
                case Keys.Left:
                    x += e.Pressed ? -Velocity : Velocity;
                    break;
                case Keys.Right:
                    x += e.Pressed ? Velocity : -Velocity;
                    break;
                default:
                    break;
            }

            if (Math.Abs(x) >= Velocity) x = Math.Sign(x) * Velocity;
            if (Math.Abs(y) >= Velocity) y = Math.Sign(y) * Velocity;

            velocity.Speed = new Vector2(x, y);
        }
    }
}
