using System.Reflection;

namespace IRO.Reflection.CodeGen
{
    public class CodeGenResult
    {
        public SourceFileContext Context { get; set; }

        public string CSharpCode { get; set; }

        public Assembly Compile()
        {
            return Compiler.Compile(
                ToCompilerInputData()
                );
        }

        public CompilerInputData ToCompilerInputData(string assemblyName=null)
        {
            var compilerInputData = new CompilerInputData
            {
                CSharpCode = CSharpCode,
                ReferencedAssemblies = Context.GetAssemblies(),
                AssemblyName=assemblyName
            };
            return compilerInputData;
        }

        
    }
}
