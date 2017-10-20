using System.Collections.Generic;
namespace PrintClassInstanceLib.Extensions
{
    public static class DictionaryExtensions
    {
        public static bool ReplaceKey(this Dictionary<string, object> dict,string oldKey, string newKey)
        {
            if (!dict.ContainsKey(oldKey) || dict.ContainsKey(newKey)) return false;
            dict.Remove(oldKey, out var val);
            dict.Add(newKey,val);
            return true;
        }
    }
}
