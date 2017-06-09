using System.Collections.Generic;

namespace PrintClassInstanceLib.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceText(this string str)
        {
            var replaceDictionary = new Dictionary<string, string>
            {
                {"System.Collections.Generic.List`1", "List"},
                {"List`1", "List"},
                {"[", "<"},
                {"]", ">"},
                {"Dictionary`2", "Dictionary"},
                {"System.String", "String"},
                {"KeyValuePair`2", "KeyValuePair"}
            };
            foreach (var kvp in replaceDictionary)
            {
                str = str.Replace(kvp.Key, kvp.Value);
            }
            return str;
        }
    }
}