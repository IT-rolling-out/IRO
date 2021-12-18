using System;

namespace IRO.LoggingExt
{
    public interface IMethodLogScope: IDisposable
    {
        /// <summary>
        /// See example of usage <a href="https://github.com/IT-rolling-out/IRO#irologgingext">here</a>.
        /// </summary>
        IMethodLogScope WithArguments(params object[] callerArguments);

        /// <summary>
        /// See example of usage <a href="https://github.com/IT-rolling-out/IRO#irologgingext">here</a>.
        /// </summary>
        T WithReturn<T>(T methodResult);

        /// <summary>
        /// See example of usage <a href="https://github.com/IT-rolling-out/IRO#irologgingext">here</a>.
        /// </summary>
        IMethodLogScope WithAdditionalValue(string name, object value);
    }
}