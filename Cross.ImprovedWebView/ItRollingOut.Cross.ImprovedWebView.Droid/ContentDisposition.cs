using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace ItRollingOut.Cross.ImprovedWebView.Droid
{
    /// <summary>
    /// Класс используется в коде для загрузки файлов. Позволяет разобрать информацию о файле.
    /// </summary>
    class ContentDisposition
    {
        private static readonly Regex RegexCheck = new Regex(
            "^([^;]+);(?:\\s*([^=]+)=((?<q>\"?)[^\"]*\\k<q>);?)*$",
            RegexOptions.Compiled
        );

        public string FileName { get; }

        public StringDictionary Parameters { get; }

        public string Type { get; }

        public ContentDisposition(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }
            Match match = RegexCheck.Match(str);
            if (!match.Success)
            {
                throw new FormatException("Input is not a valid content-disposition string.");
            }
            var typeGroup = match.Groups[1];
            var nameGroup = match.Groups[2];
            var valueGroup = match.Groups[3];

            int groupCount = match.Groups.Count;
            int paramCount = nameGroup.Captures.Count;

            Type = typeGroup.Value;
            Parameters = new StringDictionary();

            for (int i = 0; i < paramCount; i++)
            {
                string name = nameGroup.Captures[i].Value;
                string value = valueGroup.Captures[i].Value;

                if (name.Equals("filename", StringComparison.InvariantCultureIgnoreCase))
                {
                    FileName = value;
                }
                else
                {
                    Parameters.Add(name, value);
                }
            }
        }
    }
}