using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace IRO.Reflection.SummarySearch
{ 
    // http://jimblackler.net/blog/?p=49
    public static class DocsParserExtensions
    {
        public static string PrettifySummary(string summary)
        {
            var summaryLines = summary.Split('\n');
            var result = summaryLines
                .Aggregate("", (current, item) => current + (item.Trim() + "\n"))
                .Replace("summary><returns", "summary>\n<returns")
                .TrimEnd();

            return result;
        }

        public static string CommentSummary(string summary)
        {
            summary = "\n" + summary;
            return summary.Replace("\n", "\n/// ").Trim();
        }

        public static XmlDocument PrettifySummaryXml(string summary)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(PrettifySummary(summary));
            return xmlDocument;
        }

        public static string XmlSummaryToString(XmlNode xml)
        {
            try
            {
                var str = PrettifySummary(xml.InnerXml);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("<root>"+str+"</root>");

                var res = XmlGetTextOrEmpty(xmlDoc["root"], "summary");
                var returnsStr = XmlGetTextOrEmpty(xmlDoc["root"], "returns");
                if (!string.IsNullOrWhiteSpace(returnsStr))
                {
                    res += " Returns: " + returnsStr;
                }

                return res;
            }
            catch
            {
                return "";
            }
        }

        public static string GetParamsText(XmlNode xml)
        {
            var paramsStr = "";
            foreach (var item in GetParamsLines(xml))
            {
                paramsStr += " " + item + "\n";
            }
            return paramsStr;
        }

        public static string GetParamDescription(XmlNode xml, string paramName)
        {
            foreach (XmlNode item in xml.SelectNodes("param"))
            {
                var name = item.Attributes["name"].InnerText.Trim();
                var val = item.InnerText;
                if (name == paramName.Trim())
                {
                    return val;
                }
            }
            return "";
        }

        public static List<string> GetParamsLines(XmlNode xml)
        {
            var res = new List<string>();
            foreach (XmlNode item in xml.SelectNodes("param"))
            {
                var name = item.Attributes["name"].InnerText;
                var val = item.InnerText;
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(val))
                {
                    res.Add($"Param '{name}' - '{val}'.");
                }
            }

            return res;
        }

        public static string XmlGetTextOrEmpty(this XmlNode xml, string key)
        {
            string res = null;
            try
            {
                res = xml[key].InnerText;
            }
            catch
            {
            }

            return res ?? "";
        }
    }
}
