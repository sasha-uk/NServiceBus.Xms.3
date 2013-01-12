namespace NServiceBus.Xms.Pooling
{
    public interface IStore<T>
    {
        T Fetch();
        void Store(T item);
        int Count { get; }
    }
}