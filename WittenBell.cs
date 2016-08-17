using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master.Utility;

namespace Master.Algorithms
{
    /// <summary>
    /// Implementation of Witten-Bell backoff smoothing algorithm
    /// Author: Tomislav
    /// </summary>
    class WittenBell : ISmoothingAlgorithm
    {
        private int order;
        private int totalLines;
        private Mapper sequenceToIntMap;
        private List<Dictionary<List<int>, int>>[] sequenceNgrams;
        private List<Dictionary<List<int>, int[]>>[] sequenceDenominators;
        private int[] sequenceNgramsTokenCount;
        private List<Dictionary<List<int>, double[]>> ngramProbs;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="cr">Corpus reader object</param>
        /// <param name="order">Max N-gram order</param>
        /// <param name="sequenceToIntMap">Sequence to int map</param>
        /// <param name="sequenceNgrams">All sequence N-grams</param>
        /// <param name="sequenceDenominators">All sequence denominators</param>
        /// <param name="sequenceNgramsTokenCount">Sequence N-grams token count</param>
        public WittenBell(CorpusReader cr, int order, Mapper sequenceToIntMap,
            List<Dictionary<List<int>, int>>[] sequenceNgrams, List<Dictionary<List<int>, int[]>>[] sequenceDenominators,
            int[] sequenceNgramsTokenCount) {

            if (order < 1 || order > cr.GetOrder) {
                throw new NLPException("Calculation can be made up to " + cr.GetOrder + "-grams");
            }
            
            this.order = order;
            this.totalLines = cr.getNumLines();
            this.sequenceToIntMap = sequenceToIntMap;
            this.sequenceNgrams = sequenceNgrams;
            this.sequenceDenominators = sequenceDenominators;
            this.sequenceNgramsTokenCount = sequenceNgramsTokenCount;
            this.ngramProbs = new List<Dictionary<List<int>, double[]>>();
            for (int i = 0; i < order; i++) {
                ngramProbs.Add(new Dictionary<List<int>, double[]>(new ListComparer<int>()));
            }
        }

        /// <summary>
        /// Method starts calculations
        /// </summary>
        public void calculate() {
            for (int i = order-1; i >=0; i--) {
                foreach (KeyValuePair<List<int>, int> pair in sequenceNgrams[i][0]) {
                    List<int> ngram = new List<int>(pair.Key);
                    calculateProbabilities(ngram);
                }
            }
        }

        /// <summary>
        /// Method calculates N-gram probability and backoff weight
        /// </summary>
        /// <param name="ngram">N-gram</param>
        private void calculateProbabilities(List<int> ngram) {
            int size = ngram.Count;
            if (size == 3 || size == 2) {
                //calculate ngram probability
                List<int> keyDenom;
                if (size == 3) {
                    keyDenom = new List<int> { ngram[0], ngram[1] };
                }
                else {
                    keyDenom = new List<int> { ngram[0] };
                }
                int types = sequenceDenominators[size - 2][0].ContainsKey(keyDenom) ? sequenceDenominators[size - 2][0][keyDenom][0] : 0;
                int tokens = sequenceNgrams[size - 2][0].ContainsKey(keyDenom) ? sequenceNgrams[size - 2][0][keyDenom] : 0;
                if (keyDenom.Count == 1 && keyDenom[0] == -1) {
                    tokens = totalLines;
                }
                int tokenSum = sequenceDenominators[size - 2][0].ContainsKey(keyDenom) ? sequenceDenominators[size - 2][0][keyDenom][1] : 0;
                double prob = sequenceNgrams[size - 1][0][ngram] / (double)(tokens + types);
                if (ngramProbs[size - 1].ContainsKey(ngram)) {
                    ngramProbs[size - 1][ngram][0] = prob;
                }
                else {
                    ngramProbs[size - 1].Add(ngram, new double[] { prob, Double.NaN });
                }
                //calculate backoff weight
                if (!ngramProbs[size - 2].ContainsKey(keyDenom) || Double.IsNaN(ngramProbs[size - 2][keyDenom][1])) {
                    double lamda;
                    double numer = 1 - tokens / (double)(tokens + types);
                    double denom;
                    if (size == 3) {
                        List<int> subKeyDenom = new List<int> { ngram[1] };
                        denom = 1 - tokenSum / (double)(sequenceNgrams[0][0][subKeyDenom] + sequenceDenominators[0][0][subKeyDenom][0]);
                    }
                    else {
                        denom = 1 - tokenSum / (double)(sequenceNgrams[size - 2][0].Count + sequenceNgramsTokenCount[size - 2]);
                    }
                    lamda = numer / denom;
                    if (ngramProbs[size - 2].ContainsKey(keyDenom)) {
                        ngramProbs[size - 2][keyDenom][1] = lamda;
                    }
                    else {
                        ngramProbs[size - 2].Add(keyDenom, new double[] { Double.NaN, lamda });
                    }
                }
            }
            else {
                long types = sequenceNgrams[0][0].Count;
                long tokens = sequenceNgramsTokenCount[0];
                double prob;
                if (sequenceNgrams[0][0].ContainsKey(ngram)) {
                    prob =  sequenceNgrams[0][0][ngram] / (double)(tokens + types);
                }
                else {
                    prob = types / (double)((tokens + types) * (tokens - types));
                }
                if (ngramProbs[0].ContainsKey(ngram)) {
                    ngramProbs[0][ngram][0] = prob;
                }
                else {
                    ngramProbs[0].Add(ngram, new double[] { prob, Double.NaN });
                }
            }
        }

        /// <summary>
        /// Method shows results of calculations on standard output
        /// </summary>
        public void showCalculations() {
            Console.WriteLine("1-grams= " + (sequenceNgrams[0][0].Count + 1));
            for (int i = 0; i < order - 1; i++) {
                Console.WriteLine((i + 2) + "-grams" + "= " + sequenceNgrams[i+1][0].Count);
            }
            for (int i = 0; i < order; i++) {
                Console.WriteLine("\n" + (i + 1) + "-grams:");
                foreach (KeyValuePair<List<int>, double[]> pair in ngramProbs[i]) {
                    List<int> key = pair.Key;
                    double[] values = pair.Value;
                    String keyString = "";
                    for (int j = 0; j < key.Count; j++) {
                        keyString += sequenceToIntMap.getKey(key[j]) + " ";
                    }
                    Console.WriteLine(String.Format("{0,10:F6}", values[0]) + "\t"
                        + keyString + "\t" + (Double.IsNaN(values[1]) ? "" : String.Format("{0,10:F6}", values[1])));
                }
            }
        }

        /// <summary>
        /// Method returns all N-grams Witten-Bell backoff smoothed probabilities
        /// </summary>
        /// <returns>N-grams Witten-Bell backoff smoothed probabilities</returns>
        public List<Dictionary<List<int>, double[]>> getNgramProbs() {
            return this.ngramProbs;
        }
    }
}
