using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master.Utility
{
    /// <summary>
    /// Class for mapping strings to int value.
    /// Author: Tomislav
    /// </summary>
    public class Mapper
    {
        private List<Entry> entryList;
        private Entry first;
        private Entry last;

        public Mapper() {

            this.entryList = new List<Entry>();

            //create sentence begin token
            this.first = new Entry("<BOS>", -1);
            first.previous = null;
            first.next = null; ;
            //create sentence end token
            this.last = new Entry("<EOS>", -2);
            last.previous = null;
            last.next = null;
        }

        /// <summary>
        /// Method returns size of the map.
        /// </summary>
        /// <returns>Map size</returns>
        public int size() {
            return entryList.Count;
        }

        /// <summary>
        /// Method checks if key exist in map.
        /// </summary>
        /// <param name="key">Key to be checked</param>
        /// <returns>True/false depending if key was found in map.</returns>
        public bool contains(string key) {
            Entry iter = first;
            while (iter != null) {
                if (object.Equals(iter.key, key)) {
                    return true;
                }
                iter = iter.next;
            }
            return false;
        }

        /// <summary>
        /// Add string key to map. It will be mapped to current internal list int size.
        /// </summary>
        /// <param name="key">Key to be mapped</param>
        public void add(string key) {
            add(key, entryList.Count);
        }

        /// <summary>
        /// Adds string key to map. Provided value will be mapped to key.
        /// </summary>
        /// <param name="key">Key to be mapped</param>
        /// <param name="value">Key map value</param>
        public void add(string key, int value) {
            Entry newEntry = new Entry(key, value);

            if (first.next == null && last.previous == null) {
                first.next = newEntry;
                last.previous = newEntry;
                newEntry.previous = first;
                newEntry.next = last;
                entryList.Add(newEntry);
            }
            else {
                newEntry.previous = last.previous;
                last.previous.next = newEntry;
                last.previous = newEntry;
                newEntry.next = last;
                entryList.Add(newEntry);
            }
        }

        /// <summary>
        /// Method return value of a map key.
        /// </summary>
        /// <param name="key">Map key</param>
        /// <returns>Key map value or -4 if key doesn't exist</returns>
        public int getValue(string key) {
            Entry iter = first;
            while (iter != null) {
                if (object.Equals(iter.key, key)) {
                    return iter.value;
                }
                iter = iter.next;
            }
            return -1;
        }

        /// <summary>
        /// Method returns first key with given value.
        /// </summary>
        /// <param name="value">Key value</param>
        /// <returns>Returns first key with given value. If such key doesn't exist null is returned</returns>
        public string getKey(int value) {
            Entry iter = first;
            while (iter != null) {
                if (iter.value == value) {
                    return iter.key;
                }
                iter = iter.next;
            }
            return null;
        }

        /// <summary>
        /// Method returns key-value pair on given index.
        /// </summary>
        /// <param name="i">Index</param>
        /// <returns>Key-value par on given index</returns>
        public Entry getByIndex(int i) {
            return entryList[i];
        }

        /// <summary>
        /// Method return begin of sentence token
        /// </summary>
        /// <returns>Begin of sentence token</returns>
        public Entry getBOSToken() {
            return first;
        }

        /// <summary>
        /// Method return end of sentence token
        /// </summary>
        /// <returns>End of sentence token</returns>
        public Entry getEOSToken() {
            return last;
        }

        /// <summary>
        /// Method sorts map entries by their keys.
        /// </summary>
        public void sortByKey()  {
            int i, j;
            bool wasChange;
            for (i = 0, wasChange = true; wasChange; i++) {
                wasChange = false;
                for (j = 0; j < entryList.Count - 1 - i; j++) {
                    if (entryList[j + 1].key.CompareTo(entryList[j].key) < 0) {
                        Entry tmp = entryList[j];
                        entryList[j] = entryList[j + 1];
                        entryList[j + 1] = tmp;
                        wasChange = true;
                    }
                }
            }
        }

        /// <summary>
        /// Method sorts map entries by their values.
        /// </summary>
        public void sortByValue()
        {
            int i, j;
            bool wasChange;
            for (i = 0, wasChange = true; wasChange; i++) {
                wasChange = false;
                for (j = 0; j < entryList.Count - 1 - i; j++) {
                    if (entryList[j + 1].value < entryList[j].value) {
                        Entry tmp = entryList[j];
                        entryList[j] = entryList[j + 1];
                        entryList[j + 1] = tmp;
                        wasChange = true;
                    }
                }
            }
        }

        /// <summary>
        /// Prints map key-value pairs.
        /// </summary>
        public void print() {
            foreach (Entry entry in entryList) {
                Console.WriteLine("{0}: {1}", entry.key, entry.value.ToString());
            }
        }

        /// <summary>
        /// Prints map key-value pairs in order they were added.
        /// </summary>
        public void printOriginal() {
            Entry iter = first.next;
            while (!iter.key.Equals("<SE>")) {
                Console.WriteLine("{0}: {1}", iter.key, iter.value.ToString());
                iter = iter.next;
            }
        }
    }

    /// <summary>
    /// Class models one mapper entry.
    /// </summary>
    public class Entry {
        public string key;
        public int value;
        public Entry next;
        public Entry previous;

        public Entry(string key, int value) {
            this.key = key;
            this.value = value;
            this.next = null;
            this.previous = null;
        }
    }
}
