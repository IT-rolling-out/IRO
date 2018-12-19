using ItRollingOut.CmdLine;
using ItRollingOut.Storage;
using ItRollingOut.Storage.JsonFileStorage;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CmdTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var appDir = Assembly.GetExecutingAssembly().Location;
            var assemblyFileName = Path.GetFileName(appDir);
            appDir = appDir.Remove(appDir.Length - assemblyFileName.Length);
            StorageHardDrive.InitDependencies(
               new JsonLocalStorage(
                   Path.Combine(appDir, "storage.json")
                   )
               );

            //Простейшая консоль с командами из методов классса.
            CmdLineExtension.Init(new DefaultConsoleHandler());
            var cmds = new CmdSwitcher();
            cmds.PushCmdInStack(new CmdLineFacade());
            cmds.ExecuteStartup(args);
            cmds.RunDefault();
        }
    }
}
