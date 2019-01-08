using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using IRO.Common.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace IRO.Reflection.CodeGen
{
    public static class Compiler
    {
        public static Assembly Compile(CompilerInputData compilerInputData)
        {
            if (string.IsNullOrWhiteSpace(compilerInputData.CSharpCode))
            {
                throw new Exception("CSharpCode parameter can`t be null or empty.");
            }
            string assemblyName;
            if (string.IsNullOrWhiteSpace(compilerInputData.AssemblyName))
            {
                assemblyName = "CompiledAssembly_"+TextExtensions.Generate(10);
            }
            else
            {
                assemblyName = compilerInputData.AssemblyName;
            }
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(compilerInputData.CSharpCode);
            var references = GetMetadataReferences(compilerInputData.ReferencedAssemblies);

            OptimizationLevel optLevel = OptimizationLevel.Release;
#if DEBUG
            //optLevel = OptimizationLevel.Debug;
#endif
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel:optLevel
                    )

                );

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    string compileErrors = "";
                    foreach (Diagnostic diagnostic in failures)
                    {
                        compileErrors+=$"\n  *{diagnostic.Id}: {diagnostic.GetMessage()};";
                    }
                    throw new CodeGenException("Exception while trying to compile script."+compileErrors);
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());
                    return assembly;
                }
            }
        }

        static IEnumerable<MetadataReference> GetMetadataReferences(IEnumerable<Assembly> assemblies)
        {
            var assembliesList = assemblies.ToList();
            if (!assembliesList.Contains(typeof(object).Assembly))
            {
                assembliesList.Add(typeof(object).Assembly); 
            }

            var res = new List<MetadataReference>();
            foreach(var a in assembliesList)
            {
                res.Add((MetadataReference)MetadataReference.CreateFromFile(a.Location));
            }
            return res;
        }
    }
}
