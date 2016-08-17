using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master.Utility
{
    /// <summary>
    /// Class for calculating counts of counts of observations found in state.
    /// Author: Tomislav
    /// </summary>
    class CountsCalculator
    {
        private Dictionary<List<int>, int> sequenceToInt;
        private Dictionary<List<int>, int> sequenceToIntInitial;
        private SortedDictionary<int, SortedDictionary<int, int>> intStateToIntObsDictonary;

        private SortedDictionary<int, int> sequenceCoC;
        private SortedDictionary<int, int> sequenceCoCFirst;
        private SortedDictionary<int, int> sequenceCoCSecond;
        private SortedDictionary<int, int> intStateToIntObsCountDictonary;

        public CountsCalculator(Dictionary<List<int>, int> sequenceToInt, Dictionary<List<int>, int> sequenceToIntInitial, 
                    SortedDictionary<int, SortedDictionary<int, int>> intStateToIntObsDictonary) {
            this.sequenceToInt = sequenceToInt;
            this.sequenceToIntInitial = sequenceToIntInitial;
            this.intStateToIntObsDictonary = intStateToIntObsDictonary;
        }

        /// <summary>
        /// Method returns counts of counts of whole sequence
        /// </summary>
        public SortedDictionary<int, int> Get_sequenceCoC {
            get {
                return this.sequenceCoC;
            }
        }

        /// <summary>
        /// Method returnes counts of counts of first initial sequence
        /// </summary>
        public SortedDictionary<int, int> Get_sequenceCoCFirst {
            get {
                return this.sequenceCoCFirst;
            }
        }

        /// <summary>
        /// Method returnes counts of counts of second initial sequence
        /// </summary>
        public SortedDictionary<int, int> Get_sequenceCoCSecond {
            get {
                return this.sequenceCoCSecond;
            }
        }

        /// <summary>
        /// Method returns map of counts of counts
        /// </summary>
        public SortedDictionary<int, int> Get_intStateToIntObsFreqDictonary {
            get {
                return this.intStateToIntObsCountDictonary;
            }
        }

        /// <summary>
        /// Calculate counts of counts
        /// </summary>
        public void process() {
            sequenceCoC = new SortedDictionary<int, int>();
            sequenceCoCFirst = new SortedDictionary<int, int>();
            sequenceCoCSecond = new SortedDictionary<int, int>();
            intStateToIntObsCountDictonary = new SortedDictionary<int, int>();

            foreach (KeyValuePair<List<int>, int> pair in sequenceToInt) {
                int count = pair.Value;
                int countOfCounts = 0;
                sequenceCoC.TryGetValue(count, out countOfCounts);
                if (countOfCounts == 0) {
                    sequenceCoC.Add(count, countOfCounts + 1);
                }
                else {
                    sequenceCoC[count] = countOfCounts + 1;
                }
            }

            foreach (KeyValuePair<List<int>, int> pair in sequenceToIntInitial)
            {
                List<int> sequence = pair.Key;
                int count = pair.Value;
                SortedDictionary<int, int> mapToAdd = null;
                if (sequence[1] == -1) {
                    mapToAdd = sequenceCoCFirst;
                }
                else {
                    mapToAdd = sequenceCoCSecond;
                }

                int countOfCounts = 0;
                mapToAdd.TryGetValue(count, out countOfCounts);
                if (countOfCounts == 0) {
                    mapToAdd.Add(count, countOfCounts + 1);
                }
                else {
                    mapToAdd[count] = countOfCounts + 1;
                }
            }

            foreach (KeyValuePair<int, SortedDictionary<int, int>> pair in intStateToIntObsDictonary) {
                foreach(KeyValuePair<int, int> innerPair in pair.Value) {
                    int count = innerPair.Value;
                    int countOfCounts = 0;
                    intStateToIntObsCountDictonary.TryGetValue(count, out countOfCounts);
                    if (countOfCounts == 0) {
                        intStateToIntObsCountDictonary.Add(count, countOfCounts + 1);
                    }
                    else {
                        intStateToIntObsCountDictonary[count] = countOfCounts + 1;
                    }
                }
            }
        }

        /// <summary>
        /// Method returns count of counts map for given map of counts
        /// </summary>
        /// <param name="counts">Counts map</param>
        /// <returns>Count of counts map</returns>
        public static SortedDictionary<int, int> calculateCoC(SortedDictionary<int, int> counts) {
            SortedDictionary<int, int> map = new SortedDictionary<int, int>();
            foreach (KeyValuePair<int, int> pair in counts) {
                int count = pair.Value;
                if (map.ContainsKey(count)) {
                    map[count] += 1;
                }
                else {
                    map.Add(count, 1);
                }
            }
            return map;
        }
    }
}
