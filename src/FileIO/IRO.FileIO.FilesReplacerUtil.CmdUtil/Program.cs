using System;
using IRO.CmdLine;
using IRO.Storage;
using IRO.Storage.DefaultStorages;
using IRO.Storage.DefaultStorages.FileStorage;

namespace IRO.FileIO.FilesReplacerUtil.CmdUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            StorageHardDrive.InitDependencies(
               new FileStorage()
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
