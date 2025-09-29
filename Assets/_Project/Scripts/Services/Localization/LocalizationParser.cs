using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Scripts.Services.Localization
{
    public static class LocalizationParser
    {
        private const string Separator = " ~ ";
        
        public static Dictionary<string, string> ParseToDictionary(string text)
        {
            var result = new Dictionary<string, string>();

            string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                string[] columns = line.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
                
                if (columns.Length > 1)
                    result.Add(columns[0], Regex.Unescape(columns[1]));
            }

            return result;
        }
    }
}