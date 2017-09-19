namespace ScatteredLogic.Internal
{
    internal struct DeleteEntityEvent
    {
        public readonly Entity Entity;

        public DeleteEntityEvent(Entity entity)
        {
            Entity = entity;
        }
    }

    internal struct RemoveComponentEvent<T>
    {
        public readonly Entity Entity;

        public RemoveComponentEvent(Entity entity)
        {
            Entity = entity;
        }
    }

    internal struct AddComponentEvent<T>
    {
        public readonly Entity Entity;
        public readonly T Component;

        public AddComponentEvent(Entity entity, T component)
        {
            Entity = entity;
            Component = component;
        }
    }
}
