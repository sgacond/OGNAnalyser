using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Client.Util
{
    public static class StringSplitterExtensions
    {
        public static IEnumerable<string> SplitKeepDelim(this string s, string delim)
        {
            int start = 0, index;

            while ((index = s.IndexOf(delim, start)) != -1)
            {
                yield return s.Substring(start, index - start + delim.Length);
                start = index + delim.Length;
            }

            if (start < s.Length)
                yield return s.Substring(start);
        }
    }
}
