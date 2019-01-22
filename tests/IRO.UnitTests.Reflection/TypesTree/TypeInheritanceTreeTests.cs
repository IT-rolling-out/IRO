using System;
using IRO.Reflection.Core;
using NUnit.Framework;

namespace IRO.UnitTests.Reflection.TypesTree
{
    public class TypeInheritanceTreeTests
    {
        [Test]
        public void Test()
        {
            var pattern =
@"TypeGeneration_1
    TypeGeneration_1_1
        TypeGeneration_1_1_1
        TypeGeneration_1_1_2
    TypeGeneration_1_2
        TypeGeneration_1_2_1
        TypeGeneration_1_2_2
        TypeGeneration_1_2_3

TypeGeneration_2
    TypeGeneration_2_1";

            var types = TypeGenerations.GetAllGenerations();
            var treesList = TypeInheritanceTreeBuilder.BuildTrees(types);
            Func<Type, string> serializer = t => t.Name;
            string testResult = "";
            foreach (var tree in treesList)
            {
                testResult += tree.ToString(serializer);
            }

            var patternTrimmed = pattern
                .Replace("\n", "")
                .Replace("\t", "")
                .Replace(" ", "")
                .Replace("\r", "");

            var testResultTrimmed = testResult
               .Replace("\n", "")
               .Replace("\t", "")
               .Replace(" ", "")
               .Replace("\r", "");

            if (testResultTrimmed == patternTrimmed)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail("Tree doesn`t match pattern value: \n\n" + testResult);
            }
        }
    }
}
