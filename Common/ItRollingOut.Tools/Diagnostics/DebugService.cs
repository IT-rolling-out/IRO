using System.Diagnostics;

namespace ItRollingOut.Tools.Diagnostics
{
    public class DebugService : IDebugService
    {
        public void Write(object obj)
        {
            Debug.Write(obj);
        }

        public void WriteLine(object obj)
        {
            Debug.WriteLine(obj);
        }
    }
}
