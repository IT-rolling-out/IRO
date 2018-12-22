﻿using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace S2A.Plugins.WebViewSuite.Droid
{
    /// <summary>
    /// Класс используется в коде для загрузки файлов. Позволяет разобрать информацию о файле.
    /// </summary>
    class ContentDisposition
    {
        private static readonly Regex regex = new Regex(
            "^([^;]+);(?:\\s*([^=]+)=((?<q>\"?)[^\"]*\\k<q>);?)*$",
            RegexOptions.Compiled
        );

        private readonly string fileName;
        private readonly StringDictionary parameters;
        private readonly string type;

        public ContentDisposition(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException("s");
            }
            Match match = regex.Match(s);
            if (!match.Success)
            {
                throw new FormatException("Input is not a valid content-disposition string.");
            }
            var typeGroup = match.Groups[1];
            var nameGroup = match.Groups[2];
            var valueGroup = match.Groups[3];

            int groupCount = match.Groups.Count;
            int paramCount = nameGroup.Captures.Count;

            this.type = typeGroup.Value;
            this.parameters = new StringDictionary();

            for (int i = 0; i < paramCount; i++)
            {
                string name = nameGroup.Captures[i].Value;
                string value = valueGroup.Captures[i].Value;

                if (name.Equals("filename", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.fileName = value;
                }
                else
                {
                    this.parameters.Add(name, value);
                }
            }
        }
        public string FileName
        {
            get
            {
                return this.fileName;
            }
        }
        public StringDictionary Parameters
        {
            get
            {
                return this.parameters;
            }
        }
        public string Type
        {
            get
            {
                return this.type;
            }
        }
    }
}