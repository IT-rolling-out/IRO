using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using IRO.FileIO.FilesReplacerUtil.Settings;
using IRO.FileIO.ImprovedFileOperations;

namespace IRO.FileIO.FilesReplacerUtil
{

    public class FilesReplacer
    {
        FilesReplacerSettings _settings;
        string _settingsFilePath;
        string _settingsFileDirectoryPath;

        public FilesReplacer(FilesReplacerSettings settings, string settingsFilePath)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            _settingsFilePath = settingsFilePath
                ?? throw new ArgumentNullException(nameof(settingsFilePath));
            //Clone settings
            _settings = settings = (FilesReplacerSettings)settings.Clone();
            _settings.GlobalSearchSettings = _settings.GlobalSearchSettings ?? new CommonSearchSettings();
            _settings = MakePathAbsolute();
            ThrowIfWrongDirInfo(_settings);
            CompileAllRegex(_settings);

        }

        public void Copy()
        {
            var foundFilesAndDestinations = FindWithCopyDestinations();
            if (_settings.UseTransactionScope)
            {
                using (TransactionScope scope = new TransactionScope())
                {

                    foreach (var fileAndDest in foundFilesAndDestinations)
                    {
                        var sourceFile = fileAndDest.Item1;
                        var destination = fileAndDest.Item2;
                        (new ImprovedFile()).Copy(sourceFile, destination, true);
                    }
                    scope.Complete();
                }
            }
            else
            {
                foreach (var fileAndDest in foundFilesAndDestinations)
                {
                    var sourceFile = fileAndDest.Item1;
                    var destination = fileAndDest.Item2;
                    (new ImprovedFile()).TryCopy(sourceFile, destination, true);
                }
            }

        }

        public void Delete()
        {
            var foundFiles = Find();
            if (_settings.UseTransactionScope)
            {
                using (TransactionScope scope = new TransactionScope())
                {

                    foreach (var file in foundFiles)
                    {
                        (new ImprovedFile()).Delete(file);
                    }
                    scope.Complete();
                }
            }
            else
            {
                foreach (var file in foundFiles)
                {
                    (new ImprovedFile()).TryDelete(file);
                }
            }
        }

        public List<string> Find()
        {
            var res = new List<string>();
            foreach (var dir in _settings.SearchDirs)
            {
                var files = FindFiles(dir);
                res.AddRange(files);
            }
            return res;
        }

        public List<Tuple<string, string>> FindWithCopyDestinations()
        {
            var filesAndDestinations = new List<Tuple<string, string>>();
            foreach (var dir in _settings.SearchDirs)
            {
                var files = FindFiles(dir);
                foreach (var file in files)
                {
                    var filesNaming = GetNamingSettings(dir);
                    var destination = GetCopyDestinationPath(
                        file,
                        dir.DirPath,
                        filesNaming,
                        dir.OutputSubdirName
                        );
                    filesAndDestinations.Add(
                        new Tuple<string, string>(file, destination)
                        );
                }

            }
            return filesAndDestinations;
        }

        List<string> FindFiles(SearchDir dir)
        {
            var locSettings = dir.LocalSearchSettings;
            var searchRegex = new List<Regex>();
            if (locSettings?.SearchingRegexCompiled != null)
                searchRegex.AddRange(locSettings.SearchingRegexCompiled);
            if (_settings.GlobalSearchSettings?.SearchingRegexCompiled != null)
                searchRegex.AddRange(_settings.GlobalSearchSettings?.SearchingRegexCompiled);

            var ignoreRegex = new List<Regex>();
            if (locSettings?.IgnoredRegexCompiled != null)
                ignoreRegex.AddRange(locSettings.IgnoredRegexCompiled);
            if (_settings.GlobalSearchSettings?.IgnoredRegexCompiled != null)
                ignoreRegex.AddRange(_settings.GlobalSearchSettings?.IgnoredRegexCompiled);

            var files = (new ImprovedFile()).Search(
                dir.DirPath,
                searchRegex,
                ignoreRegex
                );
            return files;
        }

        FilesNamingType GetNamingSettings(SearchDir dir)
        {
            return dir.LocalSearchSettings?.FilesNaming 
                ?? _settings.GlobalSearchSettings?.FilesNaming 
                ?? default(FilesNamingType);
        }

        string GetCopyDestinationPath(string pathToFile, string rootDirFullName, FilesNamingType filesNamingType, string outputSubdirName)
        {
            if (outputSubdirName == null || string.IsNullOrWhiteSpace(outputSubdirName))
            {
                //В корне выходной папке
                outputSubdirName = "";
            }

            string mainDirPath = Path.Combine(_settings.OutputDir.DirPath, outputSubdirName);
            var destinationRelativePath = ImprovedPath.GetRelativePath(rootDirFullName, pathToFile);
            string destinationPath = null;
            if (filesNamingType == FilesNamingType.IncludePathInFileNames)
            {
                destinationPath = Path.Combine(
                    mainDirPath,
                    destinationRelativePath.Replace("\\", "__").Replace("/", "__")
                    );
            }
            else if (filesNamingType == FilesNamingType.DefaultStructure)
            {
                destinationPath = Path.Combine(
                    mainDirPath,
                    destinationRelativePath
                    );
            }
            else if (filesNamingType == FilesNamingType.AllInRoot)
            {
                destinationPath = Path.Combine(
                    mainDirPath,
                    Path.GetFileName(destinationRelativePath)
                    );
            }
            else
            {
                throw new Exception($"Unknown enum type '{filesNamingType}'.");
            }
            return destinationPath;
        }

        void CompileAllRegex(FilesReplacerSettings settings)
        {
            CompileRegex(settings.GlobalSearchSettings);

            var searchDirs = settings.SearchDirs;
            foreach (var sDir in searchDirs)
            {
                if (sDir.LocalSearchSettings!=null)
                    CompileRegex(sDir.LocalSearchSettings);
            }
        }

        void CompileRegex(CommonSearchSettings searchSettings)
        {
            if (searchSettings.SearchingRegex != null)
            {
                searchSettings.SearchingRegexCompiled =
                    searchSettings.SearchingRegex.Select(str => new Regex(str)).ToList();
            }

            if (searchSettings.IgnoredRegex != null)
            {
                searchSettings.IgnoredRegexCompiled =
                    searchSettings.IgnoredRegex.Select(str => new Regex(str)).ToList();
            }

        }

        FilesReplacerSettings MakePathAbsolute()
        {
            _settingsFilePath = Path.GetFullPath(_settingsFilePath);
            var settingsFileName = Path.GetFileName(_settingsFilePath);
            _settingsFileDirectoryPath = _settingsFilePath.Remove(
                _settingsFilePath.Length - settingsFileName.Length
                );

            //Задаем все пути относительно файла настроект.
            if (_settings.OutputDir!=null && !ImprovedPath.IsFullPath(_settings.OutputDir.DirPath))
            {
                _settings.OutputDir.DirPath = Path.Combine(
                    _settingsFileDirectoryPath,
                    _settings.OutputDir.DirPath);
            }
            for (int i = 0; i < _settings.SearchDirs.Count; i++)
            {
                if (ImprovedPath.IsFullPath(_settings.SearchDirs[i].DirPath))
                    continue;
                _settings.SearchDirs[i].DirPath = Path.Combine(
                    _settingsFileDirectoryPath,
                    _settings.SearchDirs[i].DirPath
                    );
            }
            return _settings;
        }       

        void ThrowIfWrongDirInfo(FilesReplacerSettings settings)
        {
            if (settings.OutputDir?.DirInfoMarks == null)
                return;
            foreach (var mark in settings.OutputDir.DirInfoMarks)
            {
                if (!DirInfoContains(settings.OutputDir.DirPath, mark))
                {
                    throw new Exception($"Output dir must contain dir.info with '{mark}' mark.");
                }
            }
        }


        bool DirInfoContains(string path, string mark)
        {
            if (string.IsNullOrWhiteSpace(mark))
            {
                throw new Exception("Dir info mark can`t be whitespace or null.");
            }
            try
            {
                var dirFile = Path.Combine(path, "dir.info");
                mark = mark.Trim();
                var arr = File.ReadAllText(dirFile).Split(',');
                foreach (var item in arr)
                {
                    if (item.Trim() == mark)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Dir info file is missed.", ex);
            }
        }


    }
}
