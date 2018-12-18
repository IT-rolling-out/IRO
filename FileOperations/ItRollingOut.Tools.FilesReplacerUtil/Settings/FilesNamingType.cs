using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ItRollingOut.Tools.FilesReplacerUtil.Settings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FilesNamingType
    {
        /// <summary>
        /// Путь к файлу, начиная от папки поиска остается таким же 
        /// (файл копируется вместе с папками, в которых он лежит.
        /// </summary>
        DefaultStructure = 0,

        /// <summary>
        /// Путь к файлу, начиная от папки поиска встраивается в имя файла.
        /// </summary>
        IncludePathInFileNames,

        /// <summary>
        /// Все файлы сохраняются в корень выходной папки, их путь игнорируется. 
        /// При конфликтах в имени файла записывается последний найденный файл с таким именем.
        /// </summary>
        AllInRoot
    }
}
