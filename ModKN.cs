using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master.Utility;

namespace Master.Algorithms
{
    /// <summary>
    /// Implementation of modified Kneser-Ney smoothing algorithm
    /// Author: Tomislav
    /// </summary>
    class ModKN : ISmoothingAlgorithm
    {

        private int order;
        private Dictionary<List<int>, int> sequenceToInt;
        private Mapper sequenceMap;
        private List<Dictionary<List<int>, int>> numerators;
        private List<Dictionary<List<int>, int>> denominators;
        private List<Dictionary<List<int>, Dictionary<int, int>>> notZero;
        private List<int[]> countOfCounts;
        private List<double[]> discounts;
        private Dictionary<int, int> unigramDict;
        double UnigramTokenCount = 0;
        private List<Dictionary<List<int>, double[]>> ngramProbs;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="order">Max N-gram order</param>
        /// <param name="sequenceToInt">N-grams found in input sequence</param>
        /// <param name="sequenceMap">Sequence to int map </param>
        public ModKN(int order, Dictionary<List<int>, int> sequenceToInt, Mapper sequenceMap) {
            if (order > 3 || order < 2) {
                throw new NLPException("Order must be 2 or 3");
            }

            this.order = order;
            this.sequenceToInt = sequenceToInt;
            this.sequenceMap = sequenceMap;
            this.ngramProbs = new List<Dictionary<List<int>, double[]>>();

            numerators = new List<Dictionary<List<int>, int>>();
            denominators = new List<Dictionary<List<int>, int>>();
            notZero = new List<Dictionary<List<int>, Dictionary<int, int>>>();
            unigramDict = new Dictionary<int, int>();

            for (int i = 0; i < order - 1; i++) {
                numerators.Add(new Dictionary<List<int>, int>(new ListComparer<int>()));
                denominators.Add(new Dictionary<List<int>, int>(new ListComparer<int>()));
                notZero.Add(new Dictionary<List<int>, Dictionary<int, int>>(new ListComparer<int>()));
            }

            countOfCounts = new List<int[]>();
            for (int i = 0; i < order; i++) {
                int[] entry = new int[] { 0, 0, 0, 0 };
                countOfCounts.Add(entry);
            }

            discounts = new List<double[]>();
            for (int i = 0; i < order - 1; i++) {
                double[] entry = new double[] { 0, 0, 0};
                discounts.Add(entry);
            }
        }

        /// <summary>
        /// Method starts calculations
        /// </summary>
        public void calculate() {
            foreach (KeyValuePair<List<int>, int> pair in sequenceToInt) {
                List<int> key = pair.Key;
                int ngramCount = pair.Value;
                if (key.Count > 1 && key[1] == -1) {
                    List<int> subList = new List<int>(key);
                    subList.RemoveAt(0);
                    knModCount(subList, 0, ngramCount);
                }
                else {
                    int level = key.Count == 3 ? 1 : 0;
                    knModCount(key, level, ngramCount);
                }
            }
            calcCountOfCounts();
            calcDiscounts();
            calcNgramProbs();
        }

        /// <summary>
        /// Method recursively calculates lower order N-grams, numerators 
        /// and denominators
        /// </summary>
        /// <param name="ngram">N-gram</param>
        /// <param name="i">Recursion level</param>
        /// <param name="ngramCount">N-gram frequency</param>
        private void knModCount(List<int> ngram, int i, int ngramCount, bool flag = false) {
            List<int> numer = ngram;
            List<int> denom = new List<int>(numer);
            List<int> subList = new List<int>(numer);
            denom.RemoveAt(ngram.Count - 1);
            subList.RemoveAt(0);

            Dict.addToDict(numerators[i], numer, ngramCount);
            Dict.addToDict(denominators[i], denom, ngramCount);

            if (!notZero[i].ContainsKey(denom)) {
                Dictionary<int, int> dict = new Dictionary<int, int>();
                dict.Add(ngram[ngram.Count - 1], ngramCount);
                notZero[i].Add(denom, dict);
            }
            else {
                Dict.addToDict(notZero[i][denom], ngram[ngram.Count - 1], ngramCount);
            }

            if (numerators[i][numer] == ngramCount) {
                if (i > 0) {
                    knModCount(subList, i - 1, 1);
                }
                else {
                    int unigram = ngram[ngram.Count - 1];
                    Dict.addToDict(unigramDict, unigram, 1);
                    UnigramTokenCount++;
                }
            }
        }

        /// <summary>
        /// Method calculates N-gram count of counts
        /// </summary>
        private void calcCountOfCounts() {
            foreach (KeyValuePair<int, int> pair in unigramDict) {
                if (pair.Value <= 4) {
                    countOfCounts[0][pair.Value - 1] += 1;
                }
            }
            for (int i = 0; i < numerators.Count; i++) {
                foreach (KeyValuePair<List<int>, int> pair in numerators[i]) {
                    if (pair.Value <= 4) {
                        countOfCounts[i + 1][pair.Value - 1] += 1;
                    }
                }
            }
        }

        /// <summary>
        /// Method calculates discounts
        /// </summary>
        private void calcDiscounts() {
 
            for (int i = 0; i < order - 1; i++) {
                double Y = countOfCounts[i + 1][0] / (double)(countOfCounts[i + 1][0] + 2 * countOfCounts[i + 1][1]);
                for (int j = 0; j < 3; j++) {
                    if (countOfCounts[i + 1][j] > 0) {
                        discounts[i][j] = (j + 1) - (j + 2) * Y * (countOfCounts[i + 1][j + 1] / (double)countOfCounts[i + 1][j]);
                    }
                    else {
                        discounts[i][j] = j + 1;
                    }
                }
            }
        }

        /// <summary>
        /// Method calculates all N-grams absolute discounting probabilities
        /// </summary>
        public void calcNgramProbs() {
            ngramProbs.Add(new Dictionary<List<int>, double[]>(new ListComparer<int>()));
            List<int> key = new List<int>() { -1 };
            double discount = getDiscount(0, key);
            double lamda = discount / (double)denominators[0][key];
            ngramProbs[0].Add(key, new double[] { 0, lamda });

            var list = unigramDict.Keys.ToList();
            list.Sort();

            foreach (var entry in list) {
                if (entry == -2) {
                    ngramProbs[0].Add(new List<int> { -2 }, new double[] { unigramDict[entry] / UnigramTokenCount, 0 }); 
                    continue;
                }
               
                key = new List<int>() { entry };
                discount = getDiscount(0, key);
                lamda = discount / (double)denominators[0][key];
                ngramProbs[0].Add(key, new double[] { unigramDict[entry] / UnigramTokenCount, lamda });
            }

            for (int i = 0; i < order - 2; i++) {
                ngramProbs.Add(new Dictionary<List<int>, double[]>(new ListComparer<int>()));
                foreach (KeyValuePair<List<int>, int> pair in numerators[0]) {
                    List<int> entryKey = pair.Key;
                    if (entryKey[entryKey.Count - 1] == -2) {
                        ngramProbs[i + 1].Add(entryKey, new double[] { calcProbability(entryKey), Double.NaN });
                        continue;
                    }
                    discount = getDiscount(i + 1, entryKey);
                    lamda = discount / (double)denominators[i + 1][entryKey];
                    ngramProbs[i + 1].Add(entryKey, new double[] { calcProbability(entryKey), lamda });
                }
            }
            ngramProbs.Add(new Dictionary<List<int>, double[]>(new ListComparer<int>()));
            foreach (KeyValuePair<List<int>, int> pair in numerators[order - 2]) {
                List<int> entryKey = pair.Key;
                ngramProbs[order - 1].Add(entryKey, new double[] { calcProbability(entryKey), Double.NaN });
            }
        }

        /// <summary>
        /// Method calculates single N-gram probability
        /// </summary>
        /// <param name="ngram">N-gram</param>
        /// <returns>N-gram probability</returns>
        private double calcProbability(List<int> ngram) {
            double prob = 0;
            double[] probs = new double[ngram.Count];
            for (int i = 0; i < ngram.Count; i++) {
                probs[i] = Double.MinValue;
            }
            if (ngram[ngram.Count - 1] != -1) {
                probs[0] = unigramDict[ngram[ngram.Count - 1]] / UnigramTokenCount;
            }
            for (int i = 0; i < ngram.Count - 1; i++) {
                List<int> denom = new List<int>();
                List<int> numer = new List<int>();
                for (int j = ngram.Count - (i + 2); j < ngram.Count - 1; j++) {
                    denom.Add(ngram[j]);
                    numer.Add(ngram[j]);
                }
                numer.Add(ngram[ngram.Count - 1]);

                if (denominators[i].ContainsKey(denom)) {
                    int count = numerators[i][numer];
                    int discountNum = count < 3 ? count : 3;
                    probs[i + 1] = (count - discounts[i][discountNum - 1]) / (double)denominators[i][denom];
                    double discount = getDiscount(i, denom);
                    double lamda = discount / (double)denominators[i][denom];
                    probs[i + 1] = probs[i + 1] + lamda * probs[i];
                    prob = probs[i + 1];
                }
            }
            if (Math.Abs(prob - 0) <= 1e-15) {
                prob = probs[0];
            }
            return prob;
        }

        /// <summary>
        /// Method returns discount value for given N-gram
        /// </summary>
        /// <param name="order">N-gram order</param>
        /// <param name="ngram">N-gram</param>
        /// <returns>Discount value for given N-gram</returns>
        private double getDiscount(int order, List<int> ngram) {
            int[] counts = new int[] { 0, 0, 0 };
            foreach (KeyValuePair<int, int> pair in notZero[order][ngram]) {
                if (pair.Value == 1) {
                    counts[0]++;
                }
                else if (pair.Value == 2) {
                    counts[1]++;
                }
                else {
                    counts[2]++;
                }
            }
            double discount = 0;
            for (int i = 0; i < 3; i++) {
                discount += discounts[order][i] * counts[i];
            }
            return discount;
        }

        /// <summary>
        /// Method shows results of calculations on standard output
        /// </summary>
        public void showCalculations() {
            Console.WriteLine("1-grams= " + (unigramDict.Count + 1));
            for (int i = 0; i < order - 1; i++) {
                Console.WriteLine((i + 2) + "-grams" + "= " + numerators[i].Count);
            }
            for (int i = 0; i < order; i++) {
                Console.WriteLine("\n"+(i+1)+"-grams:");
                foreach (KeyValuePair<List<int>, double[]> pair in ngramProbs[i]) {
                    List<int> key = pair.Key;
                    double[] values = pair.Value;
                    String keyString = "";
                    for (int j = 0; j < key.Count; j++) {
                        keyString += sequenceMap.getKey(key[j]) + " ";
                    }
                    Console.WriteLine(String.Format("{0,10:F6}", values[0]) + "\t"
                        + keyString + "\t" + (Double.IsNaN(values[1]) ? "" : String.Format("{0,10:F6}", values[1])));
                }
            }
        }

        /// <summary>
        /// Method returns all N-grams modified Kneser-Ney smoothed probabilities
        /// </summary>
        /// <returns>N-grams modified Kneser-Ney smoothed probabilities</returns>
        public List<Dictionary<List<int>, double[]>> getNgramProbs() {
            return this.ngramProbs;
        }
    }
}
