using ScatteredGameExample.Components;
using ScatteredGameExample.Events;
using ScatteredLogic;

namespace ScatteredGameExample.Systems
{
    public class HudSystem : BaseSystem
    {
        private Handle crosshairEntity;

        public override void Added()
        {
            crosshairEntity = EntityFactory.CreateCrosshair();

            base.Added();
            EventBus.Register<MousePositionEvent>(OnMousePosition);
        }

        private void OnMousePosition(MousePositionEvent e)
        {
            Transform t = EntityWorld.GetComponent<Transform>(crosshairEntity);
            if (t != null) t.Position = e.Position;
        }
    }
}
