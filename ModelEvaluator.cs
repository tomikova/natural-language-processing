using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master.Algorithms;
using Master.Utility;
using System.Text.RegularExpressions;

namespace Master.Model {

    /// <summary>
    /// Class containing methods for evaluating models and sentences.
    /// Calculation results are printed to standard output
    /// Author: Tomislav
    /// </summary>
    class ModelEvaluator {

        /// <summary>
        /// Method for evaluating model
        /// </summary>
        /// <param name="stateToIntMap">State to int map</param>
        /// <param name="obsToIntMap">Observation to int map</param>
        /// <param name="stateToObservation">Map of observations found in states where state is the key</param>
        /// <param name="stateNgramProbs">State N-grams probabilities</param>
        /// <param name="obsNgramProbs">Observation N-grams probabilities </param>
        /// <param name="order">Max N-gram order</param>
        /// <param name="testFile">Test file containing test sentences</param>
        public static void testModel(Mapper stateToIntMap, Mapper obsToIntMap,
            SortedDictionary<int, SortedDictionary<int, int>> stateToObservation,
            List<Dictionary<List<int>, double[]>> stateNgramProbs,
            List<Dictionary<List<int>, double[]>> obsNgramProbs, int order, String testFile) {

            //initialize Ngrams HMM
            HMM stateHMM = new HMM(stateNgramProbs);
            HMM obsHMM = new HMM(obsNgramProbs);

            //initialize viterbi
            List<List<String>[]> sentences = getTestSentences(testFile);
            Viterbi viterbi = new Viterbi(stateToIntMap, obsToIntMap, stateToObservation, stateHMM, order);

            int errors = 0;
            double totalStateProb = 0;
            int totalStateWords = 0;
            double totalObsProb = 0;
            int totalObsWords = 0;
            double totalObsProbOOV = 0;
            int totalObsWordsOOV = 0;
            int numOfSkippedWords = 0;
            int numOfSkippedSentences = 0;
            int numOfSkippedOOV = 0;
            bool skip;

            //read test file and perform calculations
            for (int i = 0; i < sentences.Count; i++) {
                List<String> obs = sentences[i][0];
                List<String> states = sentences[i][1];
                List<String> retStates = new List<String>(viterbi.process(obs.ToArray()));

                //calculating state Ngrams perplexity   
                List<int> stack = new List<int> { -1 };
                double sentProb = 0;
                for (int j = 0; j < retStates.Count; j++) {
                    int token = stateToIntMap.getValue(retStates[j]);
                    stack.Add(token);
                    double value = stateHMM.getStateToStateProb(stack);
                    sentProb += value;
                    while (true) {
                        if (stateHMM.contains(stack)) {
                            break;
                        }
                        if (stack.Count > 1) {
                            stack.RemoveAt(0);
                        }
                        else {
                            break;
                        }
                    }
                    if (stack.Count == order) {
                        stack.RemoveAt(0);
                    }
                }
                totalStateWords += states.Count;
                totalStateProb += sentProb;

                //calculating obs Ngrams perplexity         
                stack = new List<int> { -1 };
                sentProb = 0;
                int skippedWords = 0;
                skip = false;
                for (int j = 0; j < obs.Count; j++) {
                    try {
                        int token = obsToIntMap.getValue(obs[j]);
                        if (token == -1) {
                            throw new Exception();
                        }
                        stack.Add(token);
                        double value = obsHMM.getStateToStateProb(stack);
                        sentProb += value;
                        while (true) {
                            if (obsHMM.contains(stack)) {
                                break;
                            }
                            if (stack.Count > 1) {
                                stack.RemoveAt(0);
                            }
                            else {
                                break;
                            }
                        }
                        if (stack.Count == order) {
                            stack.RemoveAt(0);
                        }
                    }
                    catch (Exception) {
                        skip = true;
                        skippedWords++;
                    }
                }
                if (!skip) {
                    totalObsProb += sentProb;
                    totalObsWords += obs.Count;
                }
                else {
                    numOfSkippedWords += obs.Count;
                    numOfSkippedSentences++;
                    totalObsProbOOV += sentProb;
                    numOfSkippedOOV += skippedWords;
                    totalObsWordsOOV += (obs.Count - skippedWords);
                }

                //calculating WER
                for (int j = 0; j < retStates.Count; j++) {
                    if (!states[j].Equals(retStates[j])) {
                        errors++;
                    }
                }
            }

            //show results
            double exp = totalStateProb / totalStateWords;
            double perplexity = Math.Pow(2, -exp);
            Console.WriteLine("\nSTATE N-GRAMS RESULTS:");
            Console.WriteLine("Number of state sentences: " + sentences.Count);
            Console.WriteLine("Number of state words: " + totalStateWords);
            Console.WriteLine("Total states log2 probability: " + totalStateProb);
            Console.WriteLine("State perplexity: " + perplexity);
            Console.WriteLine("WER: " + (errors / (double)totalStateWords * 100) + "%");


            exp = totalObsProbOOV / totalObsWordsOOV;
            perplexity = Math.Pow(2, -exp);
            Console.WriteLine("\nOBS N-GRAMS RESULTS:");
            Console.WriteLine("Number of observation sentences: " + sentences.Count);
            Console.WriteLine("Number of observations in vocabulary: " + totalObsWordsOOV);
            Console.WriteLine("Number of observations out of vocabulary: " + numOfSkippedOOV);
            Console.WriteLine("Total observation log2 probability with OOV skipped: " + totalObsProbOOV);
            Console.WriteLine("Observation perplexity with OOV skipped: " + perplexity);
            exp = totalObsProb / totalObsWords;
            perplexity = Math.Pow(2, -exp);
            Console.WriteLine("\nOOV SENTENCES SKIPPED:");
            Console.WriteLine("Number of observation sentences: " + (sentences.Count - numOfSkippedSentences));
            Console.WriteLine("Number of observations: " + totalObsWords);
            Console.WriteLine("Number of skipped sentences: " + numOfSkippedSentences);
            Console.WriteLine("Number of skipped observations: " + numOfSkippedWords);
            Console.WriteLine("Total observation log2 probability: " + totalObsProb);
            Console.WriteLine("Observation perplexity: " + perplexity);
        }

        /// <summary>
        /// Method for evaluating sentence
        /// </summary>
        /// <param name="stateToIntMap">State to int map</param>
        /// <param name="obsToIntMap">Observation to int map</param>
        /// <param name="stateToObservation">Map of observations found in states where state is the key</param>
        /// <param name="stateNgramProbs">State N-grams probabilities</param>
        /// <param name="obsNgramProbs">Observation N-grams probabilities</param>
        /// <param name="order">Max N-gram order</param>
        /// <param name="sentence">Test sentence</param>
        public static void testSentence(Mapper stateToIntMap, Mapper obsToIntMap,
            SortedDictionary<int, SortedDictionary<int, int>> stateToObservation,
            List<Dictionary<List<int>, double[]>> stateNgramProbs,
            List<Dictionary<List<int>, double[]>> obsNgramProbs, int order, String sentence) {

            //initialize Ngrams HMM
            HMM stateHMM = new HMM(stateNgramProbs);
            HMM obsHMM = new HMM(obsNgramProbs);

            //initialize viterbi
            Viterbi viterbi = new Viterbi(stateToIntMap, obsToIntMap, stateToObservation, stateHMM, order);

            String[] split = Regex.Split(sentence, "\\s+");
            List<String> retStates = new List<String>(viterbi.process(split.ToArray()));

            //calculating state Ngrams perplexity   
            List<int> stack = new List<int> { -1 };
            double sentStateProb = 0;
            String stateSequence = "";
            for (int j = 0; j < retStates.Count; j++) {
                int token = stateToIntMap.getValue(retStates[j]);
                stack.Add(token);
                double value = stateHMM.getStateToStateProb(stack);
                sentStateProb += value;
                while (true) {
                    if (stateHMM.contains(stack)) {
                        break;
                    }
                    stack.RemoveAt(0);
                }
                if (stack.Count == order) {
                    stack.RemoveAt(0);
                }
                stateSequence += retStates[j] + " ";
            }

            //calculating obs Ngrams perplexity         
            stack = new List<int> { -1 };
            double sentObsProb = 0;
            int skippedWords = 0;
            for (int j = 0; j < split.Length; j++) {
                try {
                    int token = obsToIntMap.getValue(split[j]);
                    if (token == -1) {
                        throw new Exception();
                    }
                    stack.Add(token);
                    double value = obsHMM.getStateToStateProb(stack);
                    sentObsProb += value;
                    while (true) {
                        if (obsHMM.contains(stack)) {
                            break;
                        }
                        if (stack.Count > 1) {
                            stack.RemoveAt(0);
                        }
                        else {
                            break;
                        }
                    }
                    if (stack.Count == order) {
                        stack.RemoveAt(0);
                    }
                }
                catch (Exception) {
                    skippedWords++;
                }
            }

            Console.WriteLine("\nSENTENCE EVALUATION RESULTS");
            Console.WriteLine("Sentence: " + sentence);
            Console.WriteLine("Number of out of vocabulary words in sentence: " + skippedWords);
            Console.WriteLine("Most probable state sequence: " + stateSequence);
            Console.WriteLine("State sequence probability: " + Math.Pow(2,sentStateProb));
            Console.WriteLine("Sentence probability with out of vocabulary words skipped: " + Math.Pow(2,sentObsProb));

        }

        /// <summary>
        /// Method returns list of all test sentences found in test file
        /// </summary>
        /// <param name="testFile">Test file</param>
        /// <returns>List of all test sentences found in test file</returns>
        private static List<List<String>[]> getTestSentences(String testFile) {
            List<List<String>[]> sentences = new List<List<String>[]>();
            sentences.Add(new List<String>[] { new List<String>(), new List<String>() });
            using (StreamReader sr = new StreamReader(testFile)) {
                String line;
                while ((line = sr.ReadLine()) != null) {
                    String trim = line.Trim();
                    if (trim.Length == 0) {
                        List<String>[] nextSentence = new List<String>[] { new List<String>(), new List<String>() };
                        sentences.Add(nextSentence);
                    }
                    else {
                        String[] split = Regex.Split(line, "\\s+");
                        if (split.Length != 2) {
                            throw new NLPException("Test file not formatted well");
                        }
                        String obs = split[0];
                        String state = split[1];
                        sentences[sentences.Count - 1][0].Add(obs);
                        sentences[sentences.Count - 1][1].Add(state);
                    }
                }
            }
            return sentences;
        }
    }
}
