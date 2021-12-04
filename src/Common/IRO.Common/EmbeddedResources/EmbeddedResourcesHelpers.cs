using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IRO.Common.EmbeddedResources
{
    public static class EmbeddedResourcesHelpers
    {
        static IDictionary<Assembly, string[]> _manifestResourceNamesCache = new Dictionary<Assembly, string[]>();

        /// <summary> 
        /// </summary>
        /// <param name="assembly">Something like IRO.CmdLine</param>
        /// <param name="resourceName">Something like "console_script.js"</param>
        /// <returns></returns>
        public static async Task<string> ReadEmbeddedResourceText(this Assembly assembly, string resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary> 
        /// </summary>
        /// <param name="assembly">Something like IRO.CmdLine</param>
        /// <param name="resourceName">Something like "console_script.js"</param>
        /// <returns></returns>
        public static bool IsEmbeddedResourceExists(this Assembly assembly, string resourceName)
        {
            return assembly.GetManifestResourceNamesWithCache().Contains(resourceName);
        }

        /// <summary>
        /// </summary>
        /// <param name="assembly">Something like IRO.CmdLine</param>
        /// <param name="resourceName">Something like "console_script.js"</param>
        /// <param name="extractPath"></param>
        /// <returns></returns>
        public static void ExtractEmbeddedResource(this Assembly assembly, string resourceName, string extractPath)
        {
            try
            {
                var dirPath=Path.GetDirectoryName(extractPath);
                CreateDirectoryRecursively(dirPath);
                var resNames = assembly.GetManifestResourceNamesWithCache();
                if (!resNames.Contains(resourceName))
                {
                    throw new Exception("Can't find resource.");
                }
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var fileStream = File.Create(extractPath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(nameof(EmbeddedResourcesHelpers) + " error.", ex);
            }

        }

        /// <summary>
        /// NOTE: last dot in resource name always recognized as dot before file extension.
        /// </summary>
        /// <param name="assembly">Something like IRO.CmdLine</param>
        /// <param name="embeddedDirectoryPath">
        /// Something like IRO.Common.MyFolder.MySubFolder, where last two - is folders with embedded resources.
        /// Folders will be extracted too.
        /// </param>
        /// <param name="extractPath">Directory where it will be extracted.</param>
        /// <param name="ignoreErrors">Extract files which can.</param>
        /// <returns></returns>
        public static void ExtractEmbeddedResourcesDirectory(this Assembly assembly, string embeddedDirectoryPath, string extractPath, bool ignoreErrors=false)
        {
            try
            {
                CreateDirectoryRecursively(extractPath);
                var resNames = assembly.GetManifestResourceNamesWithCache();
                foreach (var resourceName in resNames)
                {
                    try
                    {
                        if (!resourceName.StartsWith(embeddedDirectoryPath))
                        {
                            continue;
                        }

                        //Remove embedded directory path part. 
                        //embeddedDirectoryPath.Length + 1 to remove dot.
                        var extractFileRelativePath = resourceName.Substring(embeddedDirectoryPath.Length + 1);
                        //Replace dots.
                        extractFileRelativePath = extractFileRelativePath.Replace(".", "/");
                        //Last slash replaced with dot, because it is extension.
                        var dotIndex = extractFileRelativePath.LastIndexOf("/");
                        var sb = new StringBuilder(extractFileRelativePath);
                        sb[dotIndex] = '.';
                        extractFileRelativePath = sb.ToString();
                        var extractFilePath = Path.Combine(extractPath, extractFileRelativePath);
                        ExtractEmbeddedResource(assembly, resourceName, extractFilePath);
                    }
                    catch
                    {
                        if (!ignoreErrors)
                            throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(nameof(EmbeddedResourcesHelpers) + " error.", ex);
            }

        }

        public static string[] GetManifestResourceNamesWithCache(this Assembly assembly)
        {
            if (!_manifestResourceNamesCache.ContainsKey(assembly))
            {
                _manifestResourceNamesCache[assembly] = assembly.GetManifestResourceNames();
            }
            return _manifestResourceNamesCache[assembly];
        }



        static void CreateDirectoryRecursively(string path)
        {
            if (Directory.Exists(path))
                return;
            var parentDirectory = Path.GetDirectoryName(path);
            if (!Directory.Exists(parentDirectory))
                CreateDirectoryRecursively(parentDirectory);
            Directory.CreateDirectory(path);
        }
    }
}
