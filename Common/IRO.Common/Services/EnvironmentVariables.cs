using System;
using System.IO;

namespace IRO.Common.Services
{
    public static class EnvironmentVariables
    {
        public static void AddToPathVariable(string newPath)
        {
            newPath = Path.GetFullPath(newPath.Trim());            
            ThrowIfPathInvalid(newPath);
            string varPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            if(!varPath.Contains(newPath))
                varPath += @";" + newPath;
            Environment.SetEnvironmentVariable("Path", varPath, EnvironmentVariableTarget.Machine);
        }

        public static void RemoveFromPathVariableValue(string newPath)
        {
            newPath = Path.GetFullPath(newPath.Trim());   
            ThrowIfPathInvalid(newPath);
            string varPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            varPath = varPath.Replace(newPath, "").Replace(";;", ";");
            Environment.SetEnvironmentVariable("Path", varPath, EnvironmentVariableTarget.Machine);
        }

        static void ThrowIfPathInvalid(string path)
        {
            try
            {
                Path.GetDirectoryName(path);
            }
            catch
            {
                throw new ArgumentException("Not valid path.", nameof(path));
            }
        }
    }
}
