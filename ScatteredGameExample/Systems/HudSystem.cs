﻿using ScatteredGameExample.Components;
using ScatteredGameExample.Events;
using ScatteredLogic;

namespace ScatteredGameExample.Systems
{
    public class HudSystem : BaseSystem
    {
        private Entity crosshairEntity;

        public override void Added()
        {
            crosshairEntity = EntityFactory.CreateCrosshair();

            base.Added();
            EventBus.Register<MousePositionEvent>(OnMousePosition);
        }

        private void OnMousePosition(MousePositionEvent e)
        {
            crosshairEntity.GetComponent<Transform>().Position = e.Position;
        }
    }
}
