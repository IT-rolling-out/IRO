using System;
using IRO.CmdLine;

namespace IRO.FileIO.FilesReplacerUtil.CmdUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            var cmds = new CmdSwitcher();
            cmds.PushCmdInStack(new CmdLineFacade());
            cmds.ExecuteStartup(args);
            if (args.Length == 0) 
                cmds.RunDefault();
            Console.ReadLine();
        }
    }


}
