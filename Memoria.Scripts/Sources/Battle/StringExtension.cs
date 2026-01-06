using System;
using System.Text.RegularExpressions;

namespace Memoria.EchoS
{
    public static class StringExtension
    {
        public static string RemoveTags(string s)
        {
            return Regex.Replace(s, "\\[[^]]*\\]", "");
        }
    }
}
