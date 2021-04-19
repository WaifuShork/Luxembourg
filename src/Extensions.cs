using System;
using System.Collections.Generic;
using System.Linq;

namespace Luxembourg
{
    public static class Extensions
    {
        public static string Substring(this string value, int startIndex, int endIndex)
        {
            return value[startIndex..endIndex];
        }

        public static TU Get<T, TU>(this Dictionary<T, TU> dictionary, T key)
        {
            return dictionary[key];
        }

        public static void Put(this Dictionary<string, object> dictionary, string key, object value)
        {
            dictionary.Put<string, object>(key, value);
        }
        
        public static void Put<T, TU>(this Dictionary<T, TU> dictionary, T key, TU value)
        {
            // does the key exist?
            // if yes, update the value at that key
            // if no, add the key and value

            if (!dictionary.ContainsKey(key))
            {
                if (!dictionary.TryAdd(key, value))
                {
                    Console.Error.WriteLine("Error: Failed to added K/V to dictionary.");
                    //throw new("Error: Failed to added K/V to dictionary.");
                }
            }
            else
            {
                try
                {
                    dictionary[key] = value;
                }
                catch (KeyNotFoundException)
                {
                    Console.Error.WriteLine("Error: Unable to update value for a nonexistent key.");
                }
            }
        }
    }
}