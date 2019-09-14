using IRO.Common.Services;
using System;
using System.Diagnostics;
using System.IO;

namespace IRO.CmdLine
{
    public static class SharedConsoleMethods
    {
        const string jsonEditorFilePath = "json_editor_buf.json";

        public static string ReadJson(string jsonPrototypeString, IConsoleHandler consoleHandler)
        {
            try
            {
                File.WriteAllText(
                    jsonEditorFilePath,
                    jsonPrototypeString
                    );

                var process = StartProcess(jsonEditorFilePath);
                process.WaitForExit();
                FileHelpers.TryReadAllText(jsonEditorFilePath, out var res, 120);
                return res;
            }
            catch (Exception ex)
            {
                consoleHandler.WriteLine(
                    $"Was error '{ex.Message}' when try to use json editor. \nBut you can write json string as default.", 
                    ConsoleColor.DarkRed
                    );
                consoleHandler.WriteLine("Or you can press enter to throw error upper.", ConsoleColor.DarkRed);
                if (!string.IsNullOrWhiteSpace(jsonPrototypeString))
                    consoleHandler.WriteLine($"Prototype: {jsonPrototypeString}", ConsoleColor.DarkYellow);
                consoleHandler.Write("Input json line: ", null);
                var res = consoleHandler.ReadLine();
                if (string.IsNullOrEmpty(res))
                {
                    throw;
                }
                else
                {
                    return res;
                }
            }
        }

        static Process StartProcess(string jsonEditorFilePath)
        {
            Process editorProcess = null;
            try
            {
                editorProcess = Process.Start(jsonEditorFilePath);
            }
            catch
            {
                editorProcess = new Process();
                editorProcess.StartNetCore(jsonEditorFilePath);
            }
            return editorProcess;

        }
    }
}
