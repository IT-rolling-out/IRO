namespace IRO.LoggingExt
{
    class EmptyDisposable : IMethodLogScope
    {
        public void Dispose()
        {
        }

        public IMethodLogScope WithArguments(params object[] callerArguments)
        {
            return this;
        }

        public T WithReturn<T>(T methodResult)
        {
            return methodResult;
        }

        public IMethodLogScope WithAdditionalValue(string name, object value)
        {
            return this;
        }
    }
}