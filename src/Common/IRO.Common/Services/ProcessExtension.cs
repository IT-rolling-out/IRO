using System.Diagnostics;

namespace IRO.Common.Services
{
    public static class ProcessExtension
    {
        /// <summary>
        /// StartProcess on .net core.
        /// Deafult Process.Start will throw exception.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="path"></param>
        public static void StartNetCore(this Process process, string path)
        {
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = ".\\" + path;
            process.Start();
        }
    }
}
