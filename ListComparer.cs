using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master.Utility {

    /// <summary>
    /// Class for comparing lists when used as map key.
    /// Author: Tomislav
    /// </summary>
    /// <typeparam name="T">List type</typeparam>
    public class ListComparer<T> : IEqualityComparer<List<T>> {

        public bool Equals(List<T> x, List<T> y) {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(List<T> obj) {
            int hashcode = 0;
            foreach (T t in obj) {
                hashcode ^= t.GetHashCode();
            }
            return hashcode;
        }
    }
}
