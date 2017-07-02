using ScatteredGameExample.Events;

namespace ScatteredGameExample.Systems
{
    public class CollisionResolverSystem : BaseSystem
    {
        public override void Added()
        {
            EventBus.Register<CollisionEvent>(OnCollision);
        }

        private void OnCollision(CollisionEvent e)
        {
            EntityWorld.DestroyEntity(e.First);
        }
    }
}
