using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;

namespace IRO.Common.Services
{
    public static class StreamHelpers
    {
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
    }
}
