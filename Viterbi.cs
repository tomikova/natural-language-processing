using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master.Algorithms;

namespace Master.Utility
{
    /// <summary>
    /// Class represents Viterbi decoder for finding most probable path through
    /// states of hidden Markov model.
    /// Author: Tomislav
    /// </summary>
    class Viterbi
    {
        private Mapper stateToIntMap;
        private Mapper obsToIntMap;
        private SortedDictionary<int, SortedDictionary<int, int>> stateToObservation;
        private SortedDictionary<int, SortedDictionary<int, double>> stateObsGTProbs;
        private HMM hmm;
        private int order;

        public Viterbi(Mapper stateToIntMap, Mapper obsToIntMap,
            SortedDictionary<int, SortedDictionary<int, int>> stateToObservation,  HMM hmm, int order) {
            this.stateToIntMap = stateToIntMap;
            this.obsToIntMap = obsToIntMap;
            this.stateToObservation = stateToObservation;
            this.hmm = hmm;
            this.order = order;
            this.stateObsGTProbs = new SortedDictionary<int, SortedDictionary<int, double>>();
            foreach (KeyValuePair<int, SortedDictionary<int, int>> pair in stateToObservation) {
                GoodTuring gt = new GoodTuring(CountsCalculator.calculateCoC(pair.Value), obsToIntMap.size());
                gt.calculate();
                stateObsGTProbs.Add(pair.Key, gt.getCountProb());
            }
        }

        /// <summary>
        /// Method processes (decodes) single sentence in one string using Viterbi algorithm
        /// </summary>
        /// <param name="sentence">Sentence with whitespace separated words</param>
        /// <returns></returns>
        public String[] process(String sentence) {
            String[] sequence = sentence.Split(null);
            return process(sequence);
        }

        /// <summary>
        /// Method processes (decodes) single sentence with words in array using Viterbi algorithm
        /// </summary>
        /// <param name="sequence">Sentence with words in array</param>
        /// <returns></returns>
        public String[] process(String[] sequence) {
            int[] obs = new  int[sequence.Length];
            int[] states = new int[sequence.Length];
            String[] ret = new String[sequence.Length];

            for (int i = 0; i < sequence.Length; i++) {
                int obsValue = obsToIntMap.getValue(sequence[i]);
                obs[i] = obsValue;
            }

            int noStates = stateToIntMap.size();
            double [,] t1 = new double [noStates, sequence.Length];
            int [,,] t2 = new int [noStates, sequence.Length, 2];
            for (int i = 0; i < noStates && sequence.Length >= 1; i++) {
                t1[i, 0] = hmm.getStateToStateProb(new List<int>() { -1, i }) + getStateToObservationSGT(i, obs[0]);
                t2[i, 0, 0] = -1;
                t2[i, 0, 1] = -1;
            }

            for (int i = 0; i < noStates && sequence.Length >= 2; i++) {
                double max = Double.NegativeInfinity;
                int index = 0;
                for (int j = 0; j < noStates; j++) {
                    List<int> key;
                    if (order == 2) {
                        key = new List<int>() { j, i };
                    }
                    else {
                        key = new List<int>() { -1, j, i };
                    }
                    double tmp = t1[j, 0] + hmm.getStateToStateProb(key) + getStateToObservationSGT(i, obs[1]);
                    if (!Double.IsNaN(tmp) && tmp > max) {
                        max = tmp;
                        index = j;
                    }
                }
                t1[i, 1] = max;
                t2[i, 1, 0] = -1;
                t2[i, 1, 1] = index;
            }

            for (int i = 2; i < sequence.Length; i++) {
                for (int j = 0; j < noStates; j++) {
                    double max = Double.NegativeInfinity;
                    int index0 = 0;
                    int index1 = 0;
                    for (int k = 0; k < noStates; k++) {
                        int tmpIndex = t2[k, i - 1, 1];
                        List<int> key;
                        if (order == 2) {
                            key = new List<int>() { k, j };
                        }
                        else {
                            key = new List<int>() { tmpIndex, k, j };
                        }
                        double tmp = t1[k, i - 1] + hmm.getStateToStateProb(key) + getStateToObservationSGT(j, obs[i]);
                        if (!Double.IsNaN(tmp) && tmp > max) {
                            max = tmp;
                            index0 = tmpIndex;
                            index1 = k;
                        }
                    }
                    t1[j, i] = max;
                    t2[j, i, 0] = index0;
                    t2[j, i, 1] = index1;
                }
            }

            double _max = Double.NegativeInfinity;
            int activeState = -1;
            for (int i = 0; i < noStates; i++) {
                if (t1[i, sequence.Length - 1] > _max) {
                    _max = t1[i, sequence.Length - 1];
                    activeState = i;
                }
            }

            if (activeState == -1) {
                return null;
            }
            states[sequence.Length - 1] = activeState;
            for (int i = sequence.Length - 1; i >= 1; i--) {
                states[i - 1] = t2[states[i],i, 1];
            }

            for (int i = 0; i < states.Length; i++) {
                ret[i] = stateToIntMap.getKey(states[i]);
            }
            return ret;
        }

        /// <summary>
        /// Method returns frequency observation probability
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="observation">State observation</param>
        /// <returns></returns>
        private double getStateToObservationFrequency(int state, int observation) {
            if (!stateToObservation.ContainsKey(state)) {
                return Math.Log(0,2);
            }
            SortedDictionary<int, int> map = stateToObservation[state];
            if (!map.ContainsKey(observation)) {
                return Math.Log(0,2);
            }
            int count = map[observation];
            int sum = 0;
            foreach (KeyValuePair<int, int> pair in map) {
                sum += pair.Value;
            }
            return Math.Log(((double)count) / sum,2);
        }

        /// <summary>
        /// Method returns Good-Turing observation probability
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="observation">State observation</param>
        /// <returns></returns>
        private double getStateToObservationSGT(int state, int observation) {
            if (!stateToObservation.ContainsKey(state)) {
                return Math.Log(0,2);
            }
            SortedDictionary<int, int> map = stateToObservation[state];
            if (!map.ContainsKey(observation)) {
                return Math.Log(stateObsGTProbs[state][0],2);
            }
            else {
                int count = map[observation];
                return Math.Log(stateObsGTProbs[state][count],2);
            }
        }
    }
}
