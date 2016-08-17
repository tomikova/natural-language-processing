using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master.Utility
{
    /// <summary>
    /// Class models hidden Markov model transition matrix
    /// Author: Tomislav
    /// </summary>
    public class HMM
    {
        private List<Dictionary<List<int>, double[]>> ngramProbs;

        /// <summary>
        /// Constructor accepting N-gram probabilities of all N-gram orders
        /// </summary>
        /// <param name="ngramProbs">N-gram probabilities</param>
        public HMM(List<Dictionary<List<int>, double[]>> ngramProbs) {
            this.ngramProbs = ngramProbs; 
        }

        /// <summary>
        /// Method sets N-gram probabilities
        /// </summary>
        /// <param name="ngramProbs">N-gram probabilities</param>
        public void setNgramProbs(List<Dictionary<List<int>, double[]>> ngramProbs) {
            this.ngramProbs = ngramProbs;
        }

        /// <summary>
        /// Method return N-gram probability
        /// </summary>
        /// <param name="ngram">N-gram</param>
        /// <returns>N-gram probability</returns>
        public double getStateToStateProb(List<int> ngram) {
            int order = ngram.Count;
            if (ngramProbs[order - 1].ContainsKey(ngram)) {
                return Math.Log(ngramProbs[order - 1][ngram][0], 2);
            }
            else {
                List<int> prefix = new List<int>(ngram);
                double bow = 0;
                while (true) {
                    prefix.RemoveAt(prefix.Count - 1);
                    if (ngramProbs[prefix.Count - 1].ContainsKey(prefix)) {
                        bow = Math.Log(ngramProbs[prefix.Count - 1][prefix][1], 2);
                        break;
                    }
                }
                List<int> lowerNgram = new List<int>(ngram);
                lowerNgram.RemoveAt(0);
                return bow + getStateToStateProb(lowerNgram);
            }
        }

        /// <summary>
        /// Method checks if N-gram exist in N-gram map
        /// </summary>
        /// <param name="ngram">N-gram</param>
        /// <returns>True/false value depending if N-gram is found</returns>
        public bool contains(List<int> ngram) {
            return ngramProbs[ngram.Count - 1].ContainsKey(ngram);
        }
    }
}
