using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ItRollingOut.Common.Services
{
    public static class TextExtensions
    {
        static char[] charsArr =  "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        static Random rnd = new Random();
        public static string Generate(int length)
        {

            char[] res = new char[length];
            for (int i = 0; i < length; i++)
                res[i] = charsArr[rnd.Next( 0, charsArr.Length - 1)];
            return new string(res);

        }

        /// <summary>
        /// Вернет истину, если в строке будут только символы латинского алфавита или числа.
        /// </summary>
        public static bool IsLatinOrNumber(string str)
        {
            foreach (char c in str.ToCharArray())
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || Char.IsNumber(c))
                    return false;
            }
            return true;
        }

        public static string ToUnderscoreCase(string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

		/// <summary>
        /// На самом деле добавляет пробелы равные одному табу. 
        /// </summary>
        /// <param name="count">Будет добавлено count*4 пробелов.</param>
        public static string AddTabs(this string str, int count = 1)
        {
            //return str;
            var tabsStr = "";
            for (int i = 0; i < count; i++)
            {
                tabsStr += "    ";
            }
            var tabsWithBreak = "\n" + tabsStr;
            return tabsStr + str.Replace("\n", tabsWithBreak);
        }
    }
}
