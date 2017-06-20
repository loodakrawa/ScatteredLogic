namespace ScatteredLogic
{
    public interface IEntitySet
    {
        int Count { get; }
        Entity this[int i] { get; }
        EntitySetEnumerator GetEnumerator();
    }
}
