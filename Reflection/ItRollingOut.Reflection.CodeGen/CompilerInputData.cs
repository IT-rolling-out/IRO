using System.Collections.Generic;
using System.Reflection;

namespace ItRollingOut.Reflection.CodeGen
{
    public struct CompilerInputData
    {
        /// <summary>
        /// Random if null.
        /// </summary>
        public string AssemblyName { get; set; }

        public string CSharpCode { get; set; }

        public IEnumerable<Assembly> ReferencedAssemblies { get; set; }
    }
}
