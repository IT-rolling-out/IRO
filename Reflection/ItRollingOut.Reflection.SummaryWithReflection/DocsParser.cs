// Копия библиотеки с некоторыми доработками 
// http://jimblackler.net/blog/?p=49

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace ItRollingOut.Reflection.SummarySearch
{

    /// <summary>
    /// Utility class to provide documentation for various types where available with the assembly.
    /// </summary>
    public static class DocsParser
    {
        /// <summary>
        /// Provides the documentation comments for a specific method
        /// </summary>
        /// <param name="methodInfo">The MethodInfo (reflection data ) of the member to find documentation for</param>
        /// <returns>The XML fragment describing the method</returns>
        public static XmlElement XmlFromMethod(MethodInfo methodInfo)
        {
            // Calculate the parameter string as this is in the member name in the XML
            string parametersString = "";
            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                if (parametersString.Length > 0)
                {
                    parametersString += ",";
                }

                parametersString += parameterInfo.ParameterType.FullName;
            }

            //Баг с поиском методов с параметрами исправлен через костыль (поиск только по StartsWith в случае ошибки).
            //Могут быть найдены неправильные комментарии в случае с перегрузками дженерик методов.
            try
            {
                if (parametersString.Length > 0)
                    return XmlFromName(methodInfo.DeclaringType, 'M', methodInfo.Name + "(" + parametersString + ")");
                else
                    return XmlFromName(methodInfo.DeclaringType, 'M', methodInfo.Name);
            }
            catch
            {
                return XmlFromName(methodInfo.DeclaringType, 'M', methodInfo.Name);
            }
        }

        public static XmlElement XmlFromProperty(PropertyInfo propertyInfo)
        {
            return XmlFromName(propertyInfo.DeclaringType, 'P', propertyInfo.Name);
        }

        public static XmlElement XmlFromMember(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
                return XmlFromProperty((PropertyInfo)memberInfo);
            if (memberInfo is Type)
                return XmlFromType((Type)memberInfo);
            return XmlFromMethod((MethodInfo)memberInfo);
        }

        /// <summary>
        /// Provides the documentation comments for a specific type
        /// </summary>
        /// <param name="type">Type to find the documentation for</param>
        /// <returns>The XML fragment that describes the type</returns>
        public static XmlElement XmlFromType(Type type)
        {
            // Prefix in type names is T
            return XmlFromName(type, 'T', "");
        }

        /// <summary>
        /// Obtains the XML Element that describes a reflection element by searching the 
        /// members for a member that has a name that describes the element.
        /// </summary>
        /// <param name="type">The type or parent type, used to fetch the assembly</param>
        /// <param name="prefix">The prefix as seen in the name attribute in the documentation XML</param>
        /// <param name="name">Where relevant, the full name qualifier for the element</param>
        /// <returns>The member that has a name that describes the specified reflection element</returns>
        private static XmlElement XmlFromName(Type type, char prefix, string name)
        {
            string fullName;

            if (String.IsNullOrEmpty(name))
            {
                fullName = prefix + ":" + type.FullName;
            }
            else
            {
                fullName = prefix + ":" + type.FullName + "." + name;
            }

            XmlDocument xmlDocument = XmlFromAssembly(type.Assembly);

            XmlElement matchedElement = null;

            foreach (XmlElement xmlElement in xmlDocument["doc"]["members"])
            {
                if (xmlElement.Attributes["name"].Value.StartsWith(fullName))
                {
                    if (matchedElement != null)
                    {
                        throw new DocsParserException("Multiple matches to query", null);
                    }

                    matchedElement = xmlElement;
                }
            }

            if (matchedElement == null)
            {
                throw new DocsParserException("Could not find documentation for specified element", null);
            }

            return matchedElement;
        }

        /// <summary>
        /// A cache used to remember Xml documentation for assemblies
        /// </summary>
        static Dictionary<string, XmlDocument> cache = new Dictionary<string, XmlDocument>();

        /// <summary>
        /// Obtains the documentation file for the specified assembly
        /// </summary>
        /// <param name="assembly">The assembly to find the XML document for</param>
        /// <returns>The XML document</returns>
        /// <remarks>This version uses a cache to preserve the assemblies, so that 
        /// the XML file is not loaded and parsed on every single lookup</remarks>
        public static XmlDocument XmlFromAssembly(Assembly assembly)
        {
            LoadAssemblyAndDocs(assembly);
            return cache[assembly.FullName];            
        }

        static void LoadAssemblyAndDocs(string assemblyPath)
        {

            string assemblyFilename = Path.GetFileName(assemblyPath);
            var asm = AssemblyIfLoaded(assemblyPath);
            if (asm == null)
            {
                asm = Assembly.LoadFrom(assemblyPath);
            }
            StreamReader streamReader;
            try
            {
                streamReader = new StreamReader(Path.ChangeExtension(assemblyPath, ".xml"));
            }
            catch (FileNotFoundException exception)
            {
                throw new DocsParserException("XML documentation not present (make sure it is turned on in project properties when building).", exception);
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(streamReader);
            cache[asm.FullName] = xmlDocument;

        }

        static void LoadAssemblyAndDocs(Assembly assembly)
        {
            var prefix = "file:///";
            LoadAssemblyAndDocs(assembly.CodeBase.Substring(prefix.Length));
        }

        /// <summary>
        /// Or null if not loaded.
        /// </summary>
        static Assembly AssemblyIfLoaded(string assemblyPath)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.FullName == assemblyPath)
                {
                    return assembly;
                }
            }
            return null;
        }
        
    }
}
