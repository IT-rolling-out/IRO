using ItRollingOut.Tools.CmdLine;
using ItRollingOut.Tools.Storage;
using ItRollingOut.Tools.Storage.JsonFileStorage;
using System;
using System.IO;

namespace ItRollingOut.Tools.FilesReplacerUtil.CmdUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            StorageHardDrive.InitDependencies(
               new JsonLocalStorage()
               );

            //Простейшая консоль с командами из методов классса.
            CmdLineExtension.Init(new DefaultConsoleHandler());
            var cmds = new CmdSwitcher();
            cmds.PushCmdInStack(new CmdLineFacade());
            cmds.ExecuteStartup(args);
            if (args.Length == 0) 
                cmds.RunDefault();
            Console.ReadLine();
        }
    }


}
