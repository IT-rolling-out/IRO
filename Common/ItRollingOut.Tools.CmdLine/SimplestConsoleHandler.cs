using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ItRollingOut.Tools.CmdLine
{
    public class SimplestConsoleHandler : IConsoleHandler
    {
        Action<string> _writeAction;

        public SimplestConsoleHandler(Action<string> writeAction)
        {
            _writeAction = writeAction;
        }

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
                _writeAction.Invoke(str);
                return;
            }

            _writeAction.Invoke(str);
        }

        public void WriteLine(string str, ConsoleColor? consoleColor)
        {
            if (consoleColor==null)
            {
                _writeAction.Invoke(str+"\n");
                return;
            }

            _writeAction.Invoke(str+"\n");
        }

        public void WriteLine()
        {
            _writeAction.Invoke("\n");
        }
    }
}
