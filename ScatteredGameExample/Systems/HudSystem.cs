using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScatteredGameExample.Components;
using ScatteredGameExample.Events;
using ScatteredLogic;

namespace ScatteredGameExample.Systems
{
    public class HudSystem : BaseSystem
    {
        private Entity crosshairEntity;
        private Vector2 offset;

        public override void Added()
        {
            crosshairEntity = EntityFactory.CreateCrosshair();
            offset = crosshairEntity.GetComponent<Texture2D>().Bounds.Size.ToVector2() / 2;

            base.Added();
            EventBus.Register<MousePositionEvent>(OnMousePosition);
        }

        private void OnMousePosition(MousePositionEvent e)
        {
            crosshairEntity.GetComponent<Transform>().Position = e.Position - offset;
        }
    }
}
