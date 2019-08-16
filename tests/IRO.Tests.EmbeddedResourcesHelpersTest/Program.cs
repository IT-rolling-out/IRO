using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using IRO.Common.Services;
using IRO.EmbeddedResources;

namespace IRO.Tests.EmbeddedResourcesHelpersTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Will try to extract embedded resources.");
            var extractTo = Path.Combine(Environment.CurrentDirectory, "ExtractedResources");
            if (Directory.Exists(extractTo))
            {
                Directory.Delete(extractTo, true);
            }
            var assembly=Assembly.GetExecutingAssembly();
            assembly.ExtractEmbeddedResourcesDirectory("IRO.Tests.EmbeddedResourcesHelpersTest.MyEmbeddedResources", extractTo);
            Process.Start(new ProcessStartInfo()
            {
                FileName = extractTo,
                UseShellExecute = true,
                Verb = "open"
            });
            Console.ReadLine();
        }
    }
}
