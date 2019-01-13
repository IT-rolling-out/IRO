using IRO.Mvc.MvcExceptionHandler.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using IRO.Reflection.Core;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public class ErrorInfoRegistry : IErrorInfoBuilder, IErrorInfoResolver
    {
        #region All data of Register method
        readonly IDictionary<Type, ErrorInfo> _modelByExTypeDict = new ConcurrentDictionary<Type, ErrorInfo>();
        readonly IDictionary<int, ErrorInfo> _modelByHttpCodeDict = new ConcurrentDictionary<int, ErrorInfo>();
        readonly IDictionary<string, ErrorInfo> _modelByErrorKeyDict = new ConcurrentDictionary<string, ErrorInfo>();
        #endregion

        #region All data of RegisterAllAssignable method
        List<AssignableErrorsInfo> _assignableErrorsInfoCollection = new List<AssignableErrorsInfo>();

        /// <summary>
        /// В этом словаре ключем выступает тип исключения, а значение указывает было ли оно (а точнее его предок)
        /// зарегистрирован через RegisterInheritTypes.
        /// </summary>
        readonly HashSet<Type> _isNotRegisterdAssignableHashSet = new HashSet<Type>();

        bool _assignableErrorsInfoCollectionIsSorted = true;
        #endregion

        readonly IErrorKeyValidator _errorKeyValidator;
        readonly IErrorKeyGenerator _errorKeyGenerator;
        readonly int _defaultHttpCode;        

        public ErrorInfoRegistry(
            IErrorKeyValidator errorKeyValidator,
            IErrorKeyGenerator errorKeyGenerator,
            int defaultHttpCode 
            )
        {
            _errorKeyValidator = errorKeyValidator;
            _errorKeyGenerator = errorKeyGenerator;
            _defaultHttpCode = defaultHttpCode;
        }

        #region Register region       
        /// <summary>
        /// Регистрирует всех наслдников типа baseExceptionType автоматически.
        /// Точнее, использует "ленивую регистрацию" проверяя тип исключения по запросу. 
        /// Из-за этого GetByErrorKey может не сработать для исключения при первом вызове.
        /// </summary>
        public void RegisterAllAssignable(AssignableErrorsInfo assignableErrorsInfo)
        {
            if (assignableErrorsInfo.BaseExceptionType == null)
                throw new ArgumentNullException(nameof(assignableErrorsInfo));
            ThrowIfTypeNotException(assignableErrorsInfo.BaseExceptionType);
            _assignableErrorsInfoCollection.Add(assignableErrorsInfo);
            _assignableErrorsInfoCollectionIsSorted = false;

            var errorInfo = new ErrorInfo();
            errorInfo.HttpCode = assignableErrorsInfo.HttpCode;
            errorInfo.ExceptionType = assignableErrorsInfo.BaseExceptionType;
            Register(errorInfo);
        }        

        public void Register(ErrorInfo errorInfo)
        {
            try
            {
                //It means if at least there only ExceptionType in model or if ErrorKey and HttpCode not null - it will work fine.
                //If not - it will throw exception, because we can`t generate ErrorKey.
                //It means that in model must be ErrorKey and ExceptionType or HttpCode. And also, for ExceptionType we can automaticallly 
                //generate ErrorKey.
                if (!(errorInfo.ExceptionType != null || (errorInfo.ErrorKey != null && errorInfo.HttpCode != null)))
                {
                    throw new ErrorHandlerException("Can`t generate ErrorKey from current model or can`t use it.");
                }

                ThrowIfTypeNotException(errorInfo.ExceptionType);

                //Generate error key if empty.
                if (string.IsNullOrWhiteSpace(errorInfo.ErrorKey))
                {
                    if (errorInfo.ExceptionType == null)
                    {
                        throw new ErrorHandlerException("Can`t automatically generate ErrorKey without exception.");
                    }
                    errorInfo.ErrorKey = _errorKeyGenerator.GenerateErrorKey(errorInfo.ExceptionType);
                }

                errorInfo.HttpCode = errorInfo.HttpCode ?? _defaultHttpCode;

                //Validate error key.
                ThrowIfErrorKeyInvalid(errorInfo.ErrorKey);

                //Throw if contains ErrorKey.
                if (_modelByErrorKeyDict.ContainsKey(errorInfo.ErrorKey))
                {
                    throw new ErrorHandlerException("You can register current ErrorKey type only once.");
                }
                else
                {
                    _modelByErrorKeyDict.Add(errorInfo.ErrorKey, errorInfo);
                }

                //Throw if contains ExceptionType.
                if (errorInfo.ExceptionType != null)
                {
                    if (_modelByExTypeDict.ContainsKey(errorInfo.ExceptionType))
                    {
                        throw new ErrorHandlerException($"Error while registering {errorInfo}.\nYou can register exception type only once.");
                    }
                    else
                    {
                        _modelByExTypeDict.Add(errorInfo.ExceptionType, errorInfo);
                    }
                }

                //!And not throw if contains HttpCode, because it can be used in one or more exceptions.
                if (_modelByHttpCodeDict.ContainsKey(errorInfo.HttpCode.Value))
                {
                    //throw new ErrorHandlerException("You can http code only once.");
                }
                else
                {
                    _modelByHttpCodeDict.Add(errorInfo.HttpCode.Value, errorInfo);
                }
            }
            catch (Exception ex)
            {
                throw new ErrorHandlerException($"Error while registering {errorInfo}.", ex);
            }
        }
        #endregion

        #region Get region
        public ErrorInfo GetByException(Type exceptionType)
        {
            ThrowIfTypeNotException(exceptionType);
            try
            {
                return _modelByExTypeDict[exceptionType];
            }
            catch (Exception ex)
            {
                const string exPatternStr = "Can`t find {0} for current exception type '{1}'.";

                if (_isNotRegisterdAssignableHashSet.Contains(exceptionType))
                {
                    //Если значение есть в _isNotRegisterdAssignableHashSet, то это исключение точно было проверено,
                    //по результату проверки - оно не было зарегистрировано через базовый тип.
                    throw new ErrorHandlerException(
                        string.Format(exPatternStr, nameof(ErrorInfo), exceptionType.Name)
                        );
                }
                else
                {
                    //Проверяем было ли исключение зарегистрировано через базовый тип.
                    var currentAssignableErrorsInfo = TryGetFirstAssignableErrorsInfo(exceptionType);

                    if (currentAssignableErrorsInfo==null)                   
                    {
                        //Если не нашли информации, то записываем что не зарегистрировано.
                        _isNotRegisterdAssignableHashSet.Add(exceptionType);
                        throw new ErrorHandlerException(
                            string.Format(exPatternStr, nameof(ErrorInfo), exceptionType.Name)
                        );
                    }
                    else
                    {
                        //Если нашли, то регистрируем.
                        var newErrorInfo = new ErrorInfo()
                        {
                            ExceptionType = exceptionType,
                            HttpCode=currentAssignableErrorsInfo.Value.HttpCode ?? _defaultHttpCode                           
                        };
                        bool registerSelfType = exceptionType == currentAssignableErrorsInfo.Value.BaseExceptionType;
                        if (!string.IsNullOrWhiteSpace(currentAssignableErrorsInfo.Value.ErrorKeyPrefix) && !registerSelfType)
                        {
                            var errorKey = _errorKeyGenerator.GenerateErrorKey(exceptionType);
                            errorKey = currentAssignableErrorsInfo.Value.ErrorKeyPrefix + errorKey;
                            newErrorInfo.ErrorKey = errorKey;
                        }
                        Register(newErrorInfo);
                        return newErrorInfo;
                    }
                }               
            }
        }        

        public ErrorInfo GetByHttCode(int httpCode)
        {
            return _modelByHttpCodeDict[httpCode];
        }

        public ErrorInfo GetByErrorKey(string errorKey)
        {
            return _modelByErrorKeyDict[errorKey];
        }
        #endregion

        public IErrorInfoResolver Build()
        {
            return this;
        }

        AssignableErrorsInfo? TryGetFirstAssignableErrorsInfo(Type inheritExceptionType)
        {
            SortAssignableErrorsIfNeeded();
            AssignableErrorsInfo? currentAssignableErrorsInfo = null;
            foreach (var item in _assignableErrorsInfoCollection)
            {
                if (item.BaseExceptionType.IsAssignableFrom(inheritExceptionType))
                {
                    currentAssignableErrorsInfo = item;
                    break;
                }
            }
            return currentAssignableErrorsInfo;
        }

        void SortAssignableErrorsIfNeeded()
        {
            lock (_assignableErrorsInfoCollection)
            {
                if (!_assignableErrorsInfoCollectionIsSorted)
                {
                    var types = _assignableErrorsInfoCollection.Select(
                        x => x.BaseExceptionType
                        );
                    var trees = TypeInheritanceTreeBuilder.BuildTrees(types);
                    var exTypesSorted = new List<Type>();
                    foreach (var tree in trees)
                    {
                        exTypesSorted.AddRange(tree.ToList());
                    }
                    //Реверсим, т.к. список отсортирован от родителей к наследникам.
                    exTypesSorted.Reverse();
                    var sortedRes=new List<AssignableErrorsInfo>();
                    foreach (var exType in exTypesSorted)
                    {
                        var assignableErrorsInfo=_assignableErrorsInfoCollection
                            .First(x=>x.BaseExceptionType== exType);
                        sortedRes.Add(assignableErrorsInfo);
                    }
                    _assignableErrorsInfoCollection = sortedRes;
                    _assignableErrorsInfoCollectionIsSorted = true;
                }
            }
        }

        void ThrowIfTypeNotException(Type type)
        {
            if (type!=null && !typeof(Exception).IsAssignableFrom(type))
                throw new ErrorHandlerException(
                    $"Argument must be Type of Exception, but current value is '{type.Name}'."
                    );
        }

        void ThrowIfErrorKeyInvalid(string errorKey)
        {
            if (!_errorKeyValidator.IsValid(errorKey))
            {
                throw new ErrorHandlerException(
                    "ErrorKey is not valid. Default validator allow you to use only '_[0-9][a-z][A-Z]'."
                    );
            }
        }
    }
}
