using System;
using IRO.CmdLine;

namespace IRO.FileIO.FilesReplacerUtil.CmdUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            //Простейшая консоль с командами из методов классса.
            var cmds = new CmdSwitcher();
            cmds.PushCmdInStack(new CmdLineFacade());
            cmds.ExecuteStartup(args);
            if (args.Length == 0) 
                cmds.RunDefault();
            Console.ReadLine();
        }
    }


}
