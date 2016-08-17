using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master.Utility
{
    /// <summary>
    /// Helper class for adding values to map
    /// Author: Tomislav
    /// </summary>
    class Dict
    {

        /// <summary>
        /// Helper method for dealing with adding values to map.
        /// </summary>
        /// <typeparam name="K">Map key type</typeparam>
        /// <typeparam name="V">Map value type</typeparam>
        /// <param name="dict">Map</param>
        /// <param name="key">Map key</param>
        /// <param name="addValue">Value to add</param>
        public static void addToDict<K, V>(Dictionary<K, V> dict, K key, V addValue) {
            V count = default(V);
            dict.TryGetValue(key, out count);
            if (count.Equals(default(V))) {
                dict.Add(key, addValue);
            }
            else {
                dynamic oldValue = count;
                dynamic newValue = addValue;
                try {
                    dict[key] = oldValue + newValue;
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    Environment.Exit(0);
                }
            }
        }
    }
}
