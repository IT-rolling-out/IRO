using IRO.Reflection.Map.Metadata;
using System;

namespace ReflectionMapTest
{
    class SubSubObj
    {
        [MethodReflectionMap]
        public void ItsToDeep(string msg)
        {
            Console.WriteLine("IT WORKS, INPUT VALUE IS " + msg);
        }

        [MethodReflectionMap]
        public int Sum(int num1, int num2,int num3)
        {
            return num1 + num2+num3;
        }
    }
}
