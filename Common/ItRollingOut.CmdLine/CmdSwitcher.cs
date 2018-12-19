using ItRollingOut.CmdLine;
using System.Collections.Generic;
using System.Linq;

namespace ItRollingOut.CmdLine
{
    public class CmdSwitcher 
    {
        Stack<CommandLineBase> cmdStack = new Stack<CommandLineBase>();
        CommandLineBase currentCmd => cmdStack.First();


        /// <summary>
        /// If not white space - will try to execute cmd.
        /// </summary>
        /// <param name="args">Process start arguments from Main(string[] args);</param>
        public void ExecuteStartup(string[] args)
        {
            string argsStr = string.Join(" ", args);
            if (string.IsNullOrWhiteSpace(argsStr))
                return;
            currentCmd.ExecuteCmd(argsStr);
        }

        public void PushCmdInStack(CommandLineBase newCmd)
        {
            cmdStack.Push(newCmd);
            currentCmd.SetCmdSwitcher(this);
            currentCmd.OnStart();
        }

        public void RunDefault()
        {
            while (cmdStack.Count > 0)
            {                           
                while (currentCmd.IsInRun)
                {
                    currentCmd.OnEveryLoop();
                }
                cmdStack.Pop();     
            }
        }
    }

}
