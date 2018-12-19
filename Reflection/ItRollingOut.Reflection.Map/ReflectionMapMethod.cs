using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ItRollingOut.Reflection.Map
{
    class ReflectionMapMethod : IReflectionMapMethod
    {
        /// <summary>
        /// Cache of MethodInfo to get generic task result.
        /// </summary>
        static ConcurrentDictionary<Type, MethodInfo> TaskReflectionGetResultCache { get; } = new ConcurrentDictionary<Type, MethodInfo>();

        public string DisplayName { get; internal set; }

        public string RealName { get; internal set; }

        public string Description { get; internal set; }

        public IReadOnlyCollection<Parameter> Parameters { get; internal set; }

        public Type ReturnType { get; internal set; }

        public MethodKind Kind { get; internal set; } = MethodKind.DefMethod;

        internal Func<object, object[], object> InvokeAction { get;  set; }

        /// <summary>
        /// Return result from called method without any manipulations on result.
        /// </summary>
        public object Execute(object instance, object[] parameters)
        {
            return InvokeAction(instance, parameters);
        }

        /// <summary>
        /// If ReturnType is task, this method will await invokation result.
        /// Else - result object will be wrapped in Task for more simple usage.
        /// </summary>
        public async Task<object> ExecuteAndAwait(object instance, object[] parameters)
        {
            object invokeRes = Execute(instance, parameters);
            
            if (typeof(Task).IsAssignableFrom(ReturnType))
            {
                var task = (Task)invokeRes;
                await task;
                return GetResultOrNullFromTask(task);
            }
            else
            { 
                return invokeRes;
            }          
        }

        object GetResultOrNullFromTask(Task task)
        {
            try
            {
                var type = task.GetType();
                MethodInfo getMethod = null;
                TaskReflectionGetResultCache.TryGetValue(type, out getMethod);
                if (getMethod == null)
                {
                    getMethod = type.GetProperty("Result").GetMethod;
                    TaskReflectionGetResultCache.TryAdd(type, getMethod);
                }                
                var res = getMethod.Invoke(task, new object[0]);
                return res;
            }
            catch
            {
                return null;
            }
        }
    }
}
