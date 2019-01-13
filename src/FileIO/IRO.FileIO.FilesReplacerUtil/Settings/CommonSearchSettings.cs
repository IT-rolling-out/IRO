using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace IRO.FileIO.FilesReplacerUtil.Settings
{
    public class CommonSearchSettings
    {
        public List<string> SearchingRegex { get; set; }

        [JsonIgnore]
        internal List<Regex> SearchingRegexCompiled { get; set; }

        public List<string> IgnoredRegex { get; set; }

        [JsonIgnore]
        internal List<Regex> IgnoredRegexCompiled { get; set; }

        public FilesNamingType? FilesNaming { get; set; } 
    }
}
