using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IRO.Reflection.CodeGen;
using NUnit.Framework;

namespace IRO.UnitTests.Reflection.CodeGen
{
    public class SourceFileContextTests
    {
        [Test]
        public void Test()
        {
            const string Source = @"
//Use types from different namespaces and assemblies.
public class GenClass1
{
    public int Num { get; set; }=100;
    
    public Stream SomeProp1 { get; set; }

    public List<string> SomeProp2 { get; set; }

    public Task<Dictionary<string, MemberInfo>> SomeMethod1(){ return null; }
}
";
            //Easy manage depemdensies.
            var ctx = new SourceFileContext();
            ctx.UsedType(typeof(int));
            ctx.UsedType(typeof(Stream));
            ctx.UsedType(typeof(List<string>));

            //Works with generic parameters automatically.
            ctx.UsedType(typeof(Task<Dictionary<string, MemberInfo>>));

            //Add namespace and usings.
            var source = CodeGenExtensions.WrapClass(
                Source,
                "MyNamespace",
                ctx.GetNamespaces()
                );
            var compilerInputData = new CompilerInputData()
            {
                CSharpCode = source,
                ReferencedAssemblies = ctx.GetAssemblies()
            };
            var assembly = Compiler.Compile(compilerInputData);
            var type = assembly.GetType("MyNamespace.GenClass1");
            dynamic modelInstance = Activator.CreateInstance(type);
            int num = modelInstance.Num;
            Assert.AreEqual(100, num);
        }
    }
}
