using System;
using IRO.Reflection;
using IRO.Reflection.Core;

namespace IRO_Tests.AlgorithmsTest.TreeAlgoriths
{
    static class TestTreeAlgoriths
    {
        public static void Test()
        {
            var types=TypeGenerations.GetAllGenerations();
            var treesList=TypeInheritanceTreeBuilder.BuildTrees(types);
            Func<Type, string> serializer = t => t.Name;
            foreach(var tree in treesList)
            {
                Console.WriteLine(tree.ToString(serializer));
            }
        }
    }
}
