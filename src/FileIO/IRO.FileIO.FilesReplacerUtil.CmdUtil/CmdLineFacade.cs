using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using IRO.CmdLine;
using IRO.Common.Services;
using IRO.FileIO.FilesReplacerUtil.Settings;
using Newtonsoft.Json;

namespace IRO.FileIO.FilesReplacerUtil.CmdUtil
{
    public class CmdLineFacade : CommandLineBase
    {
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public CmdLineFacade(CmdLineExtension cmdLineExtension = null) : base(cmdLineExtension)
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting=Formatting.Indented
            };
        }

        [CmdInfo(
            CmdName = "add_p",
            Description = "Используйте эту команду чтоб автоматически добавить в PATH ваше системы путь к этому приложению." +
                "\nЗатем вы сможете запуcкать его через командную строку написав files_replacer."
            )]
        public void AddPathVariable()
        {
#if DEBUG
            Cmd.WriteLine("Not allowed in debug mode.", ConsoleColor.Red);
            return;
#endif
            //Cmd.WriteLine("Admin rights needed.");
            RemovePathVariable();
            var assemblyFile = Assembly.GetExecutingAssembly().Location;
            var assemblyFileName = Path.GetFileName(assemblyFile);
            var appDir = assemblyFile.Remove(assemblyFile.Length - assemblyFileName.Length);
            var pathToDir = Path.Combine(appDir, "cmd");

            string script = $"\ncall dotnet \"{assemblyFile}\" %*" +
                $"\npause";

            if (!Directory.Exists(pathToDir))
            {
                Directory.CreateDirectory(pathToDir);
            }
            File.WriteAllText(
                Path.Combine(pathToDir,"files_replacer.cmd"), 
                script);
            
            EnvironmentVariables.AddToPathVariable(pathToDir);
            Cmd.WriteLine("Added to Path.");
        }

        [CmdInfo(CmdName = "rm_p")]
        public void RemovePathVariable()
        {
#if DEBUG
            Cmd.WriteLine("Not allowed in debug mode.", ConsoleColor.Red);
            return;
#endif
            Cmd.WriteLine("Admin rights needed.");
            var assemblyFile = Assembly.GetExecutingAssembly().Location;
            var assemblyFileName = Path.GetFileName(assemblyFile);
            var appDir = assemblyFile.Remove(assemblyFile.Length - assemblyFileName.Length);
            var pathToDir = Path.Combine(appDir, ".\\cmd\\");
            EnvironmentVariables.RemoveFromPathVariableValue(pathToDir);
            Cmd.WriteLine("Removed from Path.");
        }

        [CmdInfo]
        public void CreateSettingsFile(string path, string name)
        {
            if (path == null)
            {
                Cmd.WriteLine("Where you want to create settings file template?");
                path = Cmd.ReadLine().Trim();
            }
            if (name == null)
            {
                Cmd.WriteLine("Enter name of file (without extension).");
                name = Cmd.ReadLine().Trim();
            }

            var filePath = Path.Combine(path, name + ".fr.json");
            if (File.Exists(filePath))
            {
                Cmd.WriteLine("Current file exists. Overwrite?");
                bool cont=ReadResource<bool>("Continue?");
                if (!cont)
                    return;
            }
            var json=JsonConvert.SerializeObject(FilesReplacerSettings.Template(), _jsonSerializerSettings);
            File.WriteAllText(
                filePath, 
                json
                );
            Cmd.WriteLine("Created.");
        }

        [CmdInfo]
        public void Copy(string path, bool dontAsk)
        {
            var settings = ResolveSettings(path, ref path);
            var fr = new FilesReplacer(settings, path);
            Cmd.WriteLine("Searching files, please wait...");
            var foundFilesWithCopyDestinations = fr.FindWithCopyDestinations();
            var listStr = CopiedFilesListToString(foundFilesWithCopyDestinations);
            Cmd.WriteLine("Found files and their copy destinations.");
            Cmd.WriteLine(listStr);
            bool cont = dontAsk || ReadResource<bool>("Continue?");
            if (cont)
            {
                fr.Copy();
                Cmd.WriteLine("Copied.");
            }
        }

        [CmdInfo]
        public void Delete(string path, bool dontAsk)
        {
            var settings = ResolveSettings(path, ref path);
            var fr = new FilesReplacer(settings, path);
            Cmd.WriteLine("Searching files, please wait...");
            var foundFiles = fr.Find();
            Cmd.WriteLine("Files that will be deleted.");
            Cmd.WriteLine(foundFiles);
            bool cont = dontAsk || ReadResource<bool>("Continue?");
            if (cont)
            {
                fr.Delete();
                Cmd.WriteLine("Delete.");
            }
        }

        [CmdInfo]
        public void Find(string path)
        {
            var settings = ResolveSettings(path, ref path);
            var fr = new FilesReplacer(settings, path);
            Cmd.WriteLine("Searching files, please wait...");
            var foundFiles = fr.Find();
            Cmd.WriteLine("Found files.");
            Cmd.WriteLine(foundFiles);
        }

        string CopiedFilesListToString(List<Tuple<string, string>> list)
        {
            string res = "";
            foreach (var item in list)
            {
                var sourcePath = item.Item1;
                var destPath = item.Item2;
                res += sourcePath + "  --->\n" + destPath+";\n\n";
            }
            return res;
        }

        FilesReplacerSettings ResolveSettings(string settingsFile, ref string path)
        {
            string json = null;
            try
            {
                json = File.ReadAllText(settingsFile);
                var settings = JsonConvert.DeserializeObject<FilesReplacerSettings>(json, _jsonSerializerSettings);
                return settings;
            }
            catch(Exception ex)
            {
                path = Environment.CurrentDirectory;
                Cmd.WriteLine($"Error '{ex.Message}' while trying to resolve settings");
                if (json != null)
                    Cmd.WriteLine($"Loaded json '{json}'");
                Cmd.WriteLine("But you can enter it manually.");
                bool cont=ReadResource<bool>("Continue?");
                if (cont)
                {
                    var res= Cmd.ReadResource<FilesReplacerSettings>("File replacer settings");
                    Cmd.WriteLine("Now you must enter path of current settings file. " +
                        "It won`t be really created, but current path required as root for relative paths in settings.");
                    path=Cmd.ReadLine().Trim();
                    return res;
                }
                else
                {
                    throw new Exception("Can`t resolve file replacer settings.");
                }
            }
        }
    }
}
