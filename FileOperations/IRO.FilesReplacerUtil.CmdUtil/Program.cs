using IRO.CmdLine;
using IRO.Storage;
using IRO.Storage.JsonFileStorage;
using System;
using System.IO;

namespace IRO.FilesReplacerUtil.CmdUtil
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
