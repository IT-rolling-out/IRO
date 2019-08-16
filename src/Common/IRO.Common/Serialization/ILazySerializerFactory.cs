namespace IRO.Common.Serialization
{
    public interface ILazySerializerFactory
    {
        LazySerializer<T> Create<T>(T value);
    }
}
