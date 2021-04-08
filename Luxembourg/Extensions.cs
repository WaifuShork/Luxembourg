using System;
using System.Collections.Generic;

namespace Luxembourg
{
    public static class Extensions
    {
        public static string SubstringEx(this string value, int startIndex, int endIndex)
        {
            return value[startIndex..endIndex];
        }

        public static void AddOrUpdate(this Dictionary<string, TokenType> dictionary, string key, TokenType newValue)
        {
            if (dictionary.TryGetValue(key, out _))
            {
                dictionary[key] = newValue;
            }
            else
            {
                dictionary.Add(key, newValue);
            }
        }
    }
}