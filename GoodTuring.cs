using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master.Utility;

namespace Master.Algorithms
{
    /// <summary>
    /// Implementation of simple Good-Turing smoothing algorithm
    /// Author: Tomislav
    /// </summary>
    class GoodTuring : ISmoothingAlgorithm
    {
        private double CONFID_LEVEL = 1.96;
        private SortedDictionary<int, int> frequencyDictionary;

        private int[] r;
        private int[] Nr;
        private int size;
        private int Ntotal;
        private int totalSeen;
        private long vocabularySize;
        private double[] Z;
        private double[] log_r;
        private double[] log_Z;
        private double[] rSmoothed;
        private double[] P;
        private double P0Total;
        private double P0Individual;
        private double slope;
        private double intercept;
        private double smoothTot;
        private SortedDictionary<int, double> countProb;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="frequencyDictionary">Frequency counts map</param>
        /// <param name="vocabularySize">Size of vocabulary</param>
        public GoodTuring(SortedDictionary<int, int> frequencyDictionary, long vocabularySize) {
            this.frequencyDictionary = frequencyDictionary;
            this.fillData(frequencyDictionary);
            this.vocabularySize = vocabularySize;
            this.countProb = new SortedDictionary<int, double>();
        }

        /// <summary>
        /// Method starts calculations
        /// </summary>
        public void calculate() {
            Ntotal = 0;
            totalSeen = 0;
            for (int i = 0; i < frequencyDictionary.Count; i++) {
                Ntotal += r[i] * Nr[i];
                totalSeen += Nr[i];
            }

            int firstFreqIndex = findIndex(1);
            P0Total = firstFreqIndex < 0 ? 0 : Nr[firstFreqIndex] / (double)Ntotal;

            Z = sgtZ();

            log_r = new double[r.Length];
            log_Z = new double[Z.Length];

            for (int i = 0; i < r.Length; i++) {
                log_r[i] = Math.Log(r[i]);
                log_Z[i] = Math.Log(Z[i]);
            }

            LinearRegression(log_r, log_Z);

            rSmoothed = new double[r.Length];
            bool useY = false;
            for (int i = 0; i < r.Length; i++) {
                double y = (double)(r[i] + 1) * Math.Exp(slope * Math.Log(r[i] + 1) + intercept)
                    / Math.Exp(slope * Math.Log(r[i]) + intercept);
                if (findIndex(r[i] + 1) < 0) {
                    useY = true;
                }
                if (useY) {
                    rSmoothed[i] = y;
                    continue;
                }

                double Nr_current = (double)Nr[i];
                double Nr_next = (double)Nr[findIndex(r[i] + 1)];

                double x = ((double)(r[i] + 1) * Nr_next) / Nr_current;

                double t = CONFID_LEVEL * Math.Sqrt(Math.Pow((double)r[i] + 1, 2) * (Nr_next / Math.Pow(Nr_current, 2)) *
                            (1.0 + Nr_next / Nr_current));

                if (Math.Abs(x - y) > t) {
                    rSmoothed[i] = x;
                }
                else {
                    rSmoothed[i] = y;
                    useY = true;
                }
            }

            smoothTot = 0.0;

            for (int i = 0; i < rSmoothed.Length; i++) {
                smoothTot += Nr[i] * rSmoothed[i];
            }

            P = new double[r.Length];
            for (int i = 0; i < r.Length; i++) {
                P[i] = (1.0 - P0Total) * rSmoothed[i] / smoothTot;
                countProb.Add(r[i], P[i]);
            }

            P0Individual = P0Total / (vocabularySize - totalSeen);
            countProb.Add(0, P0Individual);

        }

        /// <summary>
        /// Method shows results of calculations on standard output
        /// </summary>
        public void showCalculations() {
            Console.WriteLine("P0 total: " + P0Total);
            Console.WriteLine("P0 individual: " + P0Individual);
            for (int i = 0; i < size; i++) {
                Console.WriteLine("r[" + i + "] = "+r[i]+" Nr["+i+"] = "+Nr[i]+" P["+i+"] = "+P[i]);
            }
        }

        /// <summary>
        /// Method returns count probability
        /// </summary>
        /// <returns>Count probability</returns>
        public SortedDictionary<int, double> getCountProb() {
            return countProb;
        }

        /// <summary>
        /// Method calculates Z parameter
        /// Zr=  Nr/(0.5(t-q))
        /// </summary>
        /// <returns> Z parameters</returns>
        private double[] sgtZ() {
            double[] Z = new double[frequencyDictionary.Count];

            int i, k;
            for (int index = 0; index < r.Length; index++) {
                if (index == 0) {
                    i = 0;
                }
                else {
                    i = r[index - 1];
                }
                if (index == r.Length - 1) {
                    k = 2 * r[index] - i;
                }
                else {
                    k = r[index + 1];
                }
                Z[index] = 2 * Nr[index] / (double)(k - i);
            }
            return Z;
        }

        /// <summary>
        /// Method calculates linear regression.
        /// </summary>
        /// <param name="X">X parameter</param>
        /// <param name="Y">Y parameter</param>
        private void LinearRegression(double[] X, double[] Y) {
            double meanX, meanY, numerator, denominator;
            meanX = meanY = numerator = denominator = 0.0;

            for (int i = 0; i < X.Length; i++) {
                meanX += X[i];
                meanY += Y[i];
            }
            meanX /= X.Length;
            meanY /= Y.Length;

            for (int i = 0; i < X.Length; i++) {
                numerator += X[i] * Y[i];
                denominator += X[i] * X[i];
            }
            numerator -= X.Length * meanX * meanY;
            denominator -= X.Length * meanX * meanX;
            slope = numerator / denominator;
            intercept = meanY - slope * meanX;

        }

        /// <summary>
        /// Method fills frequency and frequency count arrays
        /// </summary>
        /// <param name="frequencyDictionary">Frequency count map</param>
        private void fillData(SortedDictionary<int, int> frequencyDictionary) {
            r = new int[frequencyDictionary.Count];
            Nr = new int[frequencyDictionary.Count];
            size = frequencyDictionary.Count;

            int i = 0;
            foreach (KeyValuePair<int, int> pair in frequencyDictionary) {
                r[i] = pair.Key;
                Nr[i] = pair.Value;
                i++;
            }
        }

        /// <summary>
        /// Method finds index of given frequency
        /// </summary>
        /// <param name="freq">Frequency</param>
        /// <returns>Index of given frequency</returns>
        private int findIndex(int freq) {
            for (int i = 0; i < r.Length; i++) {
                if (r[i] == freq) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Not implemented method
        /// </summary>
        /// <returns>Raises exception</returns>
        public List<Dictionary<List<int>, double[]>> getNgramProbs() {
            throw new NLPException("Not implemented");
        }
    }
}
