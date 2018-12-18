using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ItRollingOut.Tools.CmdLine
{
    public class DefaultConsoleHandler : IConsoleHandler
    {       
        public string ReadJson(string jsonPrototypeString)
        {
            return SharedConsoleMethods.ReadJson(jsonPrototypeString, this);
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public void Write(string str, ConsoleColor? consoleColor)
        {
            if (consoleColor == null)
            {
                Console.Write(str);
                return;
            }
            var current = Console.ForegroundColor;
            Console.ForegroundColor = consoleColor.Value;
            Console.Write(str);
            Console.ForegroundColor = current;
        }

        public void WriteLine(string str, ConsoleColor? consoleColor)
        {
            if (consoleColor==null)
            {
                Console.WriteLine(str);
                return;
            }
            var current = Console.ForegroundColor;
            Console.ForegroundColor = consoleColor.Value;
            Console.WriteLine(str);
            Console.ForegroundColor = current;
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }
    }
}
