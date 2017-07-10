namespace ScatteredLogic.Internal
{
    internal struct DeleteEntityEvent
    {
        public readonly Handle Entity;

        public DeleteEntityEvent(Handle entity)
        {
            Entity = entity;
        }
    }

    internal struct RemoveComponentEvent<T>
    {
        public readonly Handle Entity;

        public RemoveComponentEvent(Handle entity)
        {
            Entity = entity;
        }
    }

    internal struct AddComponentEvent<T>
    {
        public readonly Handle Entity;
        public readonly T Component;

        public AddComponentEvent(Handle entity, T component)
        {
            Entity = entity;
            Component = component;
        }
    }
}
