﻿using System;
using System.Collections.Generic;
using System.Text;

namespace News.Core.Services.Parsing
{
    /// <summary>
    /// String extension
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Getting substring between substrings
        /// </summary>
        public static string SubstringBetweenSubstrings(this string str, string startString, string endString)
        {
            int startIndex = str.IndexOf(startString) + startString.Length;
            int endIndex = str.LastIndexOf(endString);

            if (startIndex != -1 && endIndex != -1) str = str.Substring(startIndex, endIndex - startIndex);

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
    }
}