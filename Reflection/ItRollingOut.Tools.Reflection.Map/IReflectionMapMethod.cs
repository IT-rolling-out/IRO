using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ItRollingOut.Tools.Reflection.Map
{
    /// <summary>
    /// Класс для выполнения метода.
    /// </summary>
    public interface IReflectionMapMethod
    {
        string Description { get; }

        string DisplayName { get; }

        string RealName { get; }

        /// <summary>
        /// Типы входных параметров по порядку.
        /// </summary>
        IReadOnlyCollection<Parameter> Parameters { get;}

        Type ReturnType { get; }

        MethodKind Kind { get;  }

        /// <summary>
        /// Return result from called method without any manipulations on result.
        /// </summary>
        /// <param name="instance">Экземпляр типа, для которого был построен reflection map.</param>
        /// <param name="parameters">Параметры передаваемые в метод</param>
        object Execute(object instance, object[] parameters);

        /// <summary>
        /// If ReturnType is task, this method will await invokation result.
        /// Else - result object will be wrapped in Task for more simple usage.
        /// </summary>
        /// <param name="instance">Экземпляр типа, для которого был построен reflection map.</param>
        /// <param name="parameters">Параметры передаваемые в метод</param>
        Task<object> ExecuteAndAwait(object instance, object[] parameters);
        
    }
}