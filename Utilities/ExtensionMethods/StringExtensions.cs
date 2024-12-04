using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utilities.ExtensionsMethods
{
    public static class StringExtensions
    {
        /// <summary>
        /// Clear and correct the string
        /// </summary>
        /// <param name="str"></param>
        /// <returns>The string without whitespaces and new lines</returns>
        public static string ClearWhiteSpaces(this string str)
        {
            return str?.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
        }

        /// <summary>
        /// Clear, correct the string and make all the letters low
        /// </summary>
        /// <param name="str"></param>
        /// <returns>The string without whitespaces and new lines, lowered</returns>
        public static string ClearWhiteSpacesAndToLower(this string str)
        {
            return str?.ClearWhiteSpaces().ToLower();
        }

        public static string RemoveText(this string str, string[] removeText)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            foreach (var rt in removeText)
            {
                Regex.Replace(str, rt, "", RegexOptions.IgnoreCase);
            }

            return str;
        }

    }
}