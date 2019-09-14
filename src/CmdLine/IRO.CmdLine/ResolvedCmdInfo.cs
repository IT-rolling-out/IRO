using System.Reflection;

namespace IRO.CmdLine
{
    public class ResolvedCmdInfo
    {
        public MethodInfo MethodInfo { get; set; }

        public string CmdName { get; set; }

        public string OriginalMethodName { get; set; }

        public string Description { get; set; }
    }
}