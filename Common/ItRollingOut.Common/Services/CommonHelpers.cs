using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ItRollingOut.Common.Services
{
    public static class CommonHelpers
    {
        public static string GetExecutableAssemblyDir()
        {
#if NETSTANDARD2_0
            var appDir = Assembly.GetExecutingAssembly().Location;
            var assemblyFileName = Path.GetFileName(appDir);
            appDir = appDir.Remove(appDir.Length - assemblyFileName.Length);
            return appDir;
#endif

            throw new NotSupportedException();
        }

        /// <summary>
        /// Глубокое копирование с использованием json серилизации.
        /// Использовать ОЧЕНЬ осторожно, а лучше вообще никогда.
        /// </summary>
        public static T DeepCopy<T>(object obj)
        {
            return JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(obj)
            );
        }

        public static bool TryReadAllTextFromStream(Stream stream, out string res)
        {
            try
            {
                res=ReadAllTextFromStream(stream);
                return true;
            }
            catch
            {
                res = null;
                return false;
            }
        }

        public static string ReadAllTextFromStream(Stream stream)
        {
            using (StreamReader streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();

            }
        }

        public static void CreateFileIfNotExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                string dirPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                File.CreateText(filePath).Close();
            }
        }

        public static void TryCreateFileIfNotExists(string filePath)
        {
            try
            {
                CreateFileIfNotExists(filePath);
            }
            catch { }
        }

        public static bool TryReadAllText(string filePath, out string readedText, int tryingTimeoutSeconds = 30)
        {
            return _TryReadAllText(filePath, out readedText, tryingTimeoutSeconds, DateTime.Now);
        }

        static bool _TryReadAllText(string filePath, out string readedText, int tryingTimeoutSeconds, DateTime startDT)
        {
            readedText = null;
            try
            {
                readedText = File.ReadAllText(filePath);
                return true;
            }
            catch
            {
                Thread.Sleep(1000);
                if ((DateTime.Now - startDT).Seconds < tryingTimeoutSeconds)
                {
                    return _TryReadAllText(filePath, out readedText, tryingTimeoutSeconds, startDT);
                }
            }
            return false;
        }

        public static bool TryWriteAllText(string filePath, string textToWrite, int tryingTimeoutSeconds = 30)
        {
            return _TryWriteAllText(filePath, textToWrite, tryingTimeoutSeconds, DateTime.Now);
        }

        static bool _TryWriteAllText(string filePath, string textToWrite, int tryingTimeoutSeconds, DateTime startDT)
        {
            try
            {
                File.WriteAllText(filePath, textToWrite);
                return true;
            }
            catch
            {
                Thread.Sleep(1000);
                if ((DateTime.Now - startDT).Seconds < tryingTimeoutSeconds)
                {
                    return _TryReadAllText(filePath, out textToWrite, tryingTimeoutSeconds, startDT);
                }
            }
            return false;
        }

        /// <summary> 
        /// </summary>
        /// <param name="assembly">Something like ItRollingOut.CmdLine.DroidAndBridge</param>
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
        /// <param name="assembly">Something like ItRollingOut.CmdLine.DroidAndBridge</param>
        /// <param name="resourceName">Something like "console_script.js"</param>
        /// <returns></returns>
        public static bool IsEmbededResourceExists(Assembly assembly, string resourceName)
        {
            return assembly.GetManifestResourceNames().Contains(resourceName);
        }


    }
}
