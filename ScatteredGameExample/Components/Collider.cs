namespace ScatteredGameExample.Components
{
    public enum ColliderGroup
    {
        Player,
        Bullet
    }

    public class Collider
    {
        public ColliderGroup Group { get; set; }
    }
}
