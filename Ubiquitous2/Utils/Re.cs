using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UB.Utils
{
    public static class Re
    {
        public static string GetSubString(string input, string re)
        {
            var match = Regex.Match(input, re, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            if (match.Groups.Count <= 1)
                return null;

            var result = match.Groups[1].Value;

            return String.IsNullOrEmpty(result) ? null : result;
        }
    }
}
