using System;
using System.Linq;
using System.Reflection;
using IRO.Reflection.CodeGen;
using IRO.Reflection.CodeGen.ModelsGenerators;
using IRO.Reflection.Core;
using NUnit.Framework;

namespace IRO.UnitTests.Reflection.CodeGen
{
    public class GenerateModelForMethodInfoTests
    {
        static MethodInfo Method { get; }

        static ParameterInfo[] Params { get; }

        static GenerateModelForMethodInfoTests()
        {
            Method = typeof(GenerateModelForMethodInfoTests).GetMethod(nameof(MethodToInvoke));
            Params = Method.GetParameters();

        }

        [Test]
        public void Test()
        {
            var mgi = new ModelsGeneratorInput()
            {
                ClassName = "RuntimeGeneratedModel",
                //Convert to Param.
                Params = Params.ToParam().ToList()
            };

            var modelGenerator = new ModelsGenerator(
                //Can build one or more models with one ModelsGenerator with common SourceFileContext.
                new ModelsGeneratorInput[] { mgi },
                namespaceStr: "MyGeneratedNamespace",
                baseClassName: typeof(IRuntimeGeneratedModel).FullName
                );
            var codeGenResult = modelGenerator.Generate();
            //Register used type.
            codeGenResult.Context.UsedType(typeof(IRuntimeGeneratedModel));

            var asm = codeGenResult.Compile();
            var generatedType = asm.GetType("MyGeneratedNamespace.RuntimeGeneratedModel");
            var generatedModel = (IRuntimeGeneratedModel)Activator.CreateInstance(generatedType);
            generatedModel.Num1 = 1;
            generatedModel.Num2 = 2;
            generatedModel.Num3 = 3;
            var res=MethodToInvoke(
                generatedModel.Num1,
                generatedModel.Num2,
                generatedModel.Num3
                );
            Assert.AreEqual(6, res);

        }

        public static int MethodToInvoke(int num1, int num2, int num3)
        {
            return num1 + num2 + num3;
        }
    }
}
