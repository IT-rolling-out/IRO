using System;
using System.Collections.Generic;
using IRO.CmdLine;

namespace IRO.Tests.CmdTest
{
    public class CmdLineFacade : CommandLineBase
    {
        public CmdLineFacade(CmdLineExtension cmdLineExtension = null) : base(cmdLineExtension)
        {
        }

        [CmdInfo]
        public void Test1()
        {
            //Easy read complex objects with newtonsoft json.
            //Will be opened default text editor with example value.
            var res = ReadResource<Dictionary<object, object>>("test res");
        }

        [CmdInfo(Description = "In current method you can pass parameters.")]
        public void Test2(DateTime dtParam, string strParam, bool boolParam, int intParam)
        {
            //Easy print complex objects with newtonsoft json.
            Cmd.WriteLine(new Dictionary<string, object>()
            {
                {nameof(dtParam), dtParam},
                {nameof(boolParam), boolParam},
                {nameof(strParam),strParam },
                {nameof(intParam),intParam }
            });
        }

        [CmdInfo]
        public void Test3()
        {
            var res = ReadResource<bool>("test simple res");
        }
    }
}
