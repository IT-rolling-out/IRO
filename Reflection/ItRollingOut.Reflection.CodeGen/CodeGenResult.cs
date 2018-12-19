using System.Reflection;

namespace ItRollingOut.Reflection.CodeGen
{
    public class CodeGenResult
    {
        public CsFileContext Context { get; set; }

        public string CSharpCode { get; set; }

        public Assembly Compile()
        {
            var compilerInputData = new CompilerInputData
            {
                CSharpCode = CSharpCode,
                ReferencedAssemblies = Context.GetAssemblies()
            };
            return Compiler.Compile(compilerInputData);
        }
    }
}
