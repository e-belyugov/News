using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
// ReSharper disable All

namespace News.Core.Services.Parsing
{
    /// <summary>
    /// String extension
    /// </summary>
    [SuppressMessage("ReSharper", "StringIndexOfIsCultureSpecific.1")]
    public static class StringExtension
    {
        /// <summary>
        /// Getting substring between substrings
        /// </summary>
        public static string SubstringBetweenSubstrings(this string str, string startString, string endString)
        {
            int startIndex = str.IndexOf(startString);
            if (startIndex != -1)
            {
                startIndex = startIndex + startString.Length;
                int endIndex = str.IndexOf(endString, startIndex);

                if (endIndex != -1)
                    str = str.Substring(startIndex, endIndex - startIndex);
                else 
                    str = "";
            }
            else
            {
                str = "";
            }

            return str;
        }

        /// <summary>
        /// Replacing tags with marks
        /// </summary>
        public static string ReplaceTags(this string str, string startString, string endString, string insertString)
        {
            int startIndex = str.IndexOf(startString);
            int count = 0;

            while (startIndex != -1 && count < 100)
            {
                count++;
                if (startIndex != -1)
                {
                    int endIndex = str.IndexOf(">", startIndex);

                    if (endIndex != -1)
                    {
                        string replacedString = str.Substring(startIndex, endIndex - startIndex + 1);
                        str = str.Replace(replacedString, "^" + insertString);
                    }

                    endIndex = str.IndexOf(endString);
                    if (endIndex != -1)
                    {
                        string replacedString = str.Substring(endIndex, endString.Length);
                        str = str.Replace(replacedString, insertString + "^");
                    }
                }

                startIndex = str.IndexOf(startString);
            }

            return str;
        }

        /// <summary>
        /// Removing special tags
        /// </summary>
        public static string RemoveSpecialTags(this string str)
        {
            str = str.Replace("&quot;", "\"");
            str = str.Replace("&nbsp;", " ");
            str = str.Replace("&hellip;", "...");
            str = str.Replace("&mdash;", "-");
            str = str.Replace("&laquo;", "\"");
            str = str.Replace("&raquo;", "\"");
            str = str.Replace("&#171;", "\"");
            str = str.Replace("&#187;", "\"");
            str = str.Replace("&#160;", " ");
            str = str.Replace("&#8212;", "-");
            str = str.Replace("&#8470;", "№");
            str = str.Replace("&thinsp;", " ");

            return str;
        }

        /// <summary>
        /// Replace substring between substrings
        /// </summary>
        public static string ReplaceSubstringBetweenSubstrings(this string str, string startString, string endString, string insertString)
        {
            string substring = str.SubstringBetweenSubstrings(startString, endString);
            int count = 0;

            while (substring != "" && count < 100)
            {
                count++;
                str = str.Replace(substring, insertString);
                substring = str.SubstringBetweenSubstrings(startString, endString);
            }

            return str;
        }

        /// <summary>
        /// Removing tag with content
        /// </summary>
        public static string RemoveTagWithContent(this string str, string tagName)
        {
            var substr = str.SubstringBetweenSubstrings("<" + tagName,">");
            str = str.Replace("<" + tagName + substr + ">", "");

            return str;
        }

        /// <summary>
        /// Getting first img link
        /// </summary>
        public static string GetImgHref(this string str)
        {
            var substr = str.SubstringBetweenSubstrings("<img", ">");
            if (substr != "")
                str = substr.SubstringBetweenSubstrings("src=\"", "\"");
            else
                str = "";

            return str;
        }
    }
}
