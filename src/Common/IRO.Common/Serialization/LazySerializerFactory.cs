using System;

namespace IRO.Common.Serialization
{
    public class LazySerializerFactory : ILazySerializerFactory
    {
        readonly Func<object, string> _serializer;

        public LazySerializerFactory(Func<object, string> serializer = null)
        {
            _serializer = serializer ?? LazySerializer.DefaultSerializer;
        }

        public LazySerializer<T> Create<T>(T value)
        {
            return new LazySerializer<T>(
                value,
                (obj) => _serializer?.Invoke(obj)
                );
        }
    }
}
