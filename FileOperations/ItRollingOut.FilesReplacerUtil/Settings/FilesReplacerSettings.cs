using ItRollingOut.Common.Services;
using System;
using System.Collections.Generic;

namespace ItRollingOut.FilesReplacerUtil.Settings
{
    public class FilesReplacerSettings : ICloneable
    {
        public bool UseTransactionScope { get; set; }

        public OutputDir OutputDir { get; set; }

         public CommonSearchSettings GlobalSearchSettings { get; set; }   

        public List<SearchDir> SearchDirs { get; set; }        

        public object Clone()
        {
            //Лень нормально копировать.
            return CommonHelpers.DeepCopy<FilesReplacerSettings>(this);
        }

        public static FilesReplacerSettings Template()
        {
            var frs = new FilesReplacerSettings();
            frs.OutputDir = new OutputDir();
            frs.OutputDir.DirPath = "..\\Docs";
            frs.OutputDir.DirInfoMarks = new List<string>() { "any_dir" };
            frs.GlobalSearchSettings = new CommonSearchSettings();
            frs.GlobalSearchSettings.IgnoredRegex = new List<string>()
            {
                "[Dd]ebug",
                "[Rr]elease"
            };

            frs.SearchDirs = new List<SearchDir>()
            {
                new SearchDir(),
                new SearchDir()
            };

            //
            frs.SearchDirs[0].LocalSearchSettings = new CommonSearchSettings();
            frs.SearchDirs[0].LocalSearchSettings.SearchingRegex = new List<string>()
            {
                "\\.[Mm][Dd]$",
                "[Rr][Ee][Aa][Dd][Mm][Ee]\\.[Tt][Xx][Tt]$"
            };
            frs.SearchDirs[0].DirPath = "..\\directory";
            frs.SearchDirs[0].LocalSearchSettings.FilesNaming = FilesNamingType.AllInRoot;

            //
            frs.SearchDirs[1].LocalSearchSettings = new CommonSearchSettings();
            frs.SearchDirs[1].OutputSubdirName = "testCopiedDir";
            frs.SearchDirs[1].LocalSearchSettings.SearchingRegex = new List<string>()
            {
                "[a-z]*\\.$"
            };
            frs.SearchDirs[1].LocalSearchSettings.IgnoredRegex = new List<string>()
            {
                "\\.dll"
            };
            frs.SearchDirs[1].DirPath = "E:\\test\\";
            frs.SearchDirs[1].LocalSearchSettings.FilesNaming = FilesNamingType.IncludePathInFileNames;
            return frs;
        }
    }
}
