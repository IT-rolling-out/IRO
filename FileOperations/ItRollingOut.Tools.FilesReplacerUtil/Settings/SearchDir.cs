namespace ItRollingOut.Tools.FilesReplacerUtil.Settings
{
    public class SearchDir
    {
        /// <summary>
        /// Имя папки в выходной папке, где будут хранится данные. 
        /// Если пустая строка, то сразу в корне. Если null, то имя папки будет взято с DirPath.
        /// </summary>
        public string OutputSubdirName { get; set; }

        public string DirPath { get; set; }

        public CommonSearchSettings LocalSearchSettings { get; set; }        
    }
}
