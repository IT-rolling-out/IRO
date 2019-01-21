using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IRO.Common.Services
{
    public static class EmbededResourcesHelpers
    {
        /// <summary> 
        /// </summary>
        /// <param name="assembly">Something like IRO.CmdLine.DroidAndBridge</param>
        /// <param name="resourceName">Something like "console_script.js"</param>
        /// <returns></returns>
        public static async Task<string> ReadEmbededResourceText(Assembly assembly, string resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary> 
        /// </summary>
        /// <param name="assembly">Something like IRO.CmdLine.DroidAndBridge</param>
        /// <param name="resourceName">Something like "console_script.js"</param>
        /// <returns></returns>
        public static bool IsEmbededResourceExists(Assembly assembly, string resourceName)
        {
            return assembly.GetManifestResourceNames().Contains(resourceName);
        }
    }
}
