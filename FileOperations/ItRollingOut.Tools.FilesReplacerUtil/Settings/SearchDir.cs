namespace ItRollingOut.Tools.FilesReplacerUtil.Settings
{
    public class SearchDir
    {
        /// <summary>
        /// ��� ����� � �������� �����, ��� ����� �������� ������. 
        /// ���� ������ ������, �� ����� � �����. ���� null, �� ��� ����� ����� ����� � DirPath.
        /// </summary>
        public string OutputSubdirName { get; set; }

        public string DirPath { get; set; }

        public CommonSearchSettings LocalSearchSettings { get; set; }        
    }
}
