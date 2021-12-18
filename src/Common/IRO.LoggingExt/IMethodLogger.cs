using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace IRO.LoggingExt
{
    public interface IMethodLogger
    {
        /// <summary>
        /// See example of usage <a href="https://github.com/IT-rolling-out/IRO#irologgingext">here</a>.
        /// </summary>
        IMethodLogScope MethodLogScope(
            ILogger logger = null,
            [CallerMemberName] string methodName = null
        );
    }
}