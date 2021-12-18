using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace IRO.LoggingExt
{
    class MethodLogScope : IMethodLogScope
    {
        readonly ILogger _logger;
        readonly MethodLoggingSettings _settings;
        readonly MethodBase _methodInfo;
        ExecutionContext _currentExecutionContext;
        Exception _resolvedException;
        object[] _callerArguments;
        object _methodResult;
        bool _methodResultIsSet;
        IDictionary<string, object> _additionalValues = new Dictionary<string, object>();

        public MethodLogScope(ILogger logger, MethodLoggingSettings settings, MethodBase methodInfo)
        {
            _settings = settings;
            if (_settings.LogLevel > LogLevel.Error)
            {
                return;
            }

            _currentExecutionContext = Thread.CurrentThread.ExecutionContext;
            AppDomain.CurrentDomain.FirstChanceException += FirstChanceExceptionHandler;
            _logger = logger;
            _methodInfo = methodInfo;
        }

        public IMethodLogScope WithArguments(params object[] callerArguments)
        {
            if (_settings.LogLevel > LogLevel.Error)
            {
                return this;
            }

            _callerArguments = callerArguments;
            return this;
        }

        public T WithReturn<T>(T methodResult)
        {
            if (_settings.LogLevel > LogLevel.Error)
            {
                return methodResult;
            }

            _methodResult = methodResult;
            _methodResultIsSet = true;
            return methodResult;
        }

        public IMethodLogScope WithAdditionalValue(string name, object value)
        {
            if (_settings.LogLevel > LogLevel.Error)
            {
                return this;
            }

            _additionalValues[name] = value;
            return this;
        }

        public void Dispose()
        {
            try
            {
                if (_settings.LogLevel > LogLevel.Error)
                    return;

                if (_resolvedException == null && _settings.LogLevel > LogLevel.Information)
                    return;


                //Collect default log data
                var callerClassName = _methodInfo.ReflectedType?.Name;
                var callerNamespace = _methodInfo.ReflectedType?.Namespace;
                var methodName = _methodInfo.Name;

                var logMsgString = "Method {CallerNamespase}.{CallerClass}.{CalledMethod} called.";
                var logMsgParams = new List<object>()
                {
                    callerClassName,
                    callerNamespace,
                    methodName
                };

                //Collect method arguments.
                var methodParamsInfo = _methodInfo.GetParameters();
                if (_callerArguments?.Length > 0)
                {
                    logMsgString += "\n\nWith arguments: ";
                    if (_callerArguments.Length == methodParamsInfo.Length)
                    {
                        for (var i = 0; i < _callerArguments.Length; i++)
                        {
                            var pi = methodParamsInfo[i];
                            var value = _callerArguments[i];
                            logMsgString += "{Argument_" + pi.Name + "}, ";
                            logMsgParams.Add(value);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < _callerArguments.Length; i++)
                        {
                            var value = _callerArguments[i];
                            logMsgString += "{Argument_" + i + "}, ";
                            logMsgParams.Add(value);
                        }
                    }
                    logMsgString = logMsgString.Remove(logMsgString.Length - 2) + ".";
                }

                //Callect additional values.
                if (_additionalValues.Count > 0)
                {
                    logMsgString += "\n\nWith additional values: ";

                    foreach (var item in _additionalValues)
                    {
                        logMsgString += "{" + item.Key + "}, ";
                        logMsgParams.Add(item.Value);
                    }
                    logMsgString = logMsgString.Remove(logMsgString.Length - 2) + ".";
                }

                //Collect result.
                if (_methodResultIsSet)
                {
                    logMsgString += "\n\nReturned: {Result}.";
                    logMsgParams.Add(_methodResult);
                }

                var logLevel = LogLevel.Information;

                //Collect exception.
                if (_resolvedException != null)
                {
                    logMsgString += "\n\nFinished with exception: {Exception}";
                    logMsgParams.Add(_resolvedException);
                    logLevel = LogLevel.Error;
                }

                var array = logMsgParams.ToArray();
                _logger.Log(logLevel, logMsgString, args: array);
            }
            finally
            {
                AppDomain.CurrentDomain.FirstChanceException -= FirstChanceExceptionHandler;
                _currentExecutionContext = null;
            }
        }

        void FirstChanceExceptionHandler(object source, FirstChanceExceptionEventArgs args)
        {
            if (_currentExecutionContext == Thread.CurrentThread.ExecutionContext)
            {
                AppDomain.CurrentDomain.FirstChanceException -= FirstChanceExceptionHandler;
                _resolvedException = args.Exception;
            }
        }
    }
}