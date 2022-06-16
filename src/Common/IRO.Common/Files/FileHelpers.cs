using System;
using System.IO;
using System.Threading;
using IRO.Common.Services;

namespace IRO.Common.Files
{
    public static class FileHelpers
    {
        #region Try read|write.
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
        #endregion
    }
}
