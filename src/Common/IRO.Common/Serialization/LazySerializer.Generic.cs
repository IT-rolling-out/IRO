using System;

namespace IRO.Common.Serialization
{
    public struct LazySerializer<T>
    {
        Func<T, string> _serializer;

        string _serializedObject;

        public T Value { get; }

        /// <summary>
        /// Default serializer is JsonConvert.
        /// </summary>
        public LazySerializer(T value)
        {
            Value = value;
            _serializer = LazySerializer.DefaultSerializer;
            _serializedObject = null;
        }

        /// <summary>
        /// Default serializer is JsonConvert.
        /// </summary>
        public LazySerializer(T value, Func<T, string> serializer)
        {
            Value = value;
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            _serializer = serializer;
            _serializedObject = null;
        }

        public override string ToString()
        {
            if (_serializedObject == null)
            {
                if (_serializer == null)
                    _serializer = LazySerializer.DefaultSerializer;
                _serializedObject = _serializer.Invoke(Value);
            }
            return _serializedObject;
        }


    }
}
