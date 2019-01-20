using System;
using IRO.Reflection.Map.Metadata;

namespace IRO.SlnTests.ReflectionMapTest
{
    class SubObj
    {
        [IncludedObjReflectionMap]
        public SubSubObj AnotherIncludedObj { get; } = new SubSubObj();

        [MethodReflectionMap]
        public void HiWorld(int param)
        {

        }

        [MethodReflectionMap]
        public void Foo(DateTime dtParam, string strParam, bool boolParam, int intParam)
        {

        }
    }
}
