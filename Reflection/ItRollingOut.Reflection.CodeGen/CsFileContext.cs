using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ItRollingOut.Reflection.CodeGen
{
    public class CsFileContext
    {
        readonly HashSet<Type> _types = new HashSet<Type>();

        readonly HashSet<Assembly> _additionalAssemblies = new HashSet<Assembly>();

        readonly HashSet<string> _additionalNamespaces = new HashSet<string>();

        /// <summary>
        /// Сливает контекст cs файлов в один.
        /// </summary>
        public static CsFileContext Merge(params CsFileContext[] contextEnumerable)
        {
            var resContext = new CsFileContext();
            foreach (var ctx in contextEnumerable)
            {
                foreach (var item in ctx._types)
                {
                    if (resContext._types.Contains(item))
                        continue;
                    resContext._types.Add(item);
                }
                foreach (var item in ctx._additionalAssemblies)
                {
                    if (resContext._additionalAssemblies.Contains(item))
                        continue;
                    resContext._additionalAssemblies.Add(item);
                }
                foreach (var item in ctx._additionalNamespaces)
                {
                    if (resContext._additionalNamespaces.Contains(item))
                        continue;
                    resContext._additionalNamespaces.Add(item);
                }
            }
            return resContext;
        }

        #region Add objects
        /// <summary>
        /// Регистрирует переданный тип и все его внутренние generic параметры как используемые в данном файле.
        /// </summary>
        public void UsedType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsGenericType)
            {
                var genericArgs =type.GetGenericArguments();              
                for (var i = 0; i < genericArgs.Length; i++)
                {
                    UsedType(genericArgs[0]);
                }
            }

            if (_types.Contains(type))
            {
                return;
            }
            _types.Add(type);
        }

        /// <summary>
        /// Регистрирует сборку как используемую. 
        /// </summary>
        public void UsedAssembly(Assembly assembly)
        {
            if (assembly==null)
                throw new ArgumentNullException(nameof(assembly));
            if (_additionalAssemblies.Contains(assembly))
            {
                return;
            }
            _additionalAssemblies.Add(assembly);
        }

        /// <summary>
        /// Регистрирует неймспейс в списке используемых в этом файле.
        /// </summary>
        public void UsedNamespace(string namespaceStr)
        {
            if (string.IsNullOrWhiteSpace(namespaceStr))
                throw new Exception("Namespace string can`t be null or empty.");
            namespaceStr = namespaceStr.Trim();
            if (_additionalNamespaces.Contains(namespaceStr))
            {
                return;
            }
            _additionalNamespaces.Add(namespaceStr);
        }
        #endregion

        #region Resolve context data
        /// <summary>
        /// Возвращает список использованных типов.
        /// </summary>
        public ICollection<Type> GetTypes()
        {
            return _types.ToList();
        }

        /// <summary>
        /// Возвращает список использованных сборок, включая сборки типов добавленных через UsedType и UsedAssembly.
        /// </summary>
        public ICollection<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly>();
            foreach (var asm in _additionalAssemblies)
            {
                assemblies.Add(asm);
            }

            foreach (var type in _types)
            {
                var asm = type.Assembly;           
                if (assemblies.Contains(asm))
                    continue;
                assemblies.Add(asm);
            }

            return assemblies;
        }

        /// <summary>
        /// Возвращает все неймспейсы для типов добавленных через UsedType, а также неймспейсы из UsedNamespace.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetNamespaces()
        {
            var namespaces = new HashSet<string>();
            foreach (var ns in _additionalNamespaces)
            {
                namespaces.Add(ns);
            }

            foreach (var type in _types)
            {
                var ns = type.Namespace;
                if (namespaces.Contains(ns))
                    continue;
                namespaces.Add(ns);
            }          

            return namespaces;
        }
        #endregion
    }
}
