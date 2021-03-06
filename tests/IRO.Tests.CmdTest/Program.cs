﻿using IRO.CmdLine;

namespace IRO.Tests.CmdTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //Простейшая консоль с командами из методов классса.
            var cmds = new CmdSwitcher();
            cmds.PushCmdInStack(new CmdLineFacade());
            cmds.ExecuteStartup(args);
            cmds.RunDefault();
        }
    }
}
