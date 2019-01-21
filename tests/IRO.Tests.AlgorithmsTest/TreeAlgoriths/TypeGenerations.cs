using System;
using System.Collections.Generic;

namespace IRO.Tests.AlgorithmsTest.TreeAlgoriths
{
    static class TypeGenerations
    {
        public static List<Type> GetAllGenerations()
        {
            return new List<Type>()
            {
                typeof(TypeGeneration_1),
                typeof(TypeGeneration_1_1),
                typeof(TypeGeneration_1_2),
                typeof(TypeGeneration_1_1_1),
                typeof(TypeGeneration_1_1_2),
                typeof(TypeGeneration_1_2_1),
                typeof(TypeGeneration_1_2_2),
                typeof(TypeGeneration_1_2_3),
                typeof(TypeGeneration_2),
                typeof(TypeGeneration_2_1)
            };
        }
    }

    class TypeGeneration_1
    {
    }

    class TypeGeneration_1_1 : TypeGeneration_1
    {
    }

    class TypeGeneration_1_2 : TypeGeneration_1
    {
    }

    class TypeGeneration_1_1_1 : TypeGeneration_1_1
    {
    }

    class TypeGeneration_1_1_2 : TypeGeneration_1_1
    {
    }

    class TypeGeneration_1_2_1 : TypeGeneration_1_2
    {
    }

    class TypeGeneration_1_2_2 : TypeGeneration_1_2
    {
    }

    class TypeGeneration_1_2_3 : TypeGeneration_1_2
    {
    }

    class TypeGeneration_2
    {
    }

    class TypeGeneration_2_1:TypeGeneration_2
    {
    }
}
