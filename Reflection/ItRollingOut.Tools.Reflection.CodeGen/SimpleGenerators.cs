﻿using System.Collections.Generic;
using ItRollingOut.Tools.Services;

namespace ItRollingOut.Tools.Reflection.CodeGen
{
    public static class SimpleGenerators
    {
        public static string WrapClass(string innerCode, string codeNamespace, IEnumerable<string> usings=null)
        {
            var res = "//!Autogenerated code.\n";
            if (usings != null)
            {
                foreach (var usingStr in usings)
                {
                    res += "using " + usingStr + ";\n";
                }
            }
            res += "namespace " +
                codeNamespace +
                "\n{\n" +
                innerCode.AddTabs() +
                "\n}";
            return res;

        }
    }
}
