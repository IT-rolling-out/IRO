using System.Diagnostics;

namespace ItRollingOut.Tools.Services
{
    public static class ProcessExtension
    {
        /// <summary>
        /// Запуск процессов в .net core.
        /// Обычный Process.Start не сработает.
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
