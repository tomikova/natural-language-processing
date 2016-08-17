using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master.Algorithms
{
    /// <summary>
    /// Interface defining methods every smoothing algorithm must implement
    /// Author: Tomislav
    /// </summary>
    interface ISmoothingAlgorithm
    {

        /// <summary>
        /// Method starts smoothing calculations
        /// </summary>
        void calculate();

        /// <summary>
        /// Method shows results of calculations on standard output
        /// </summary>
        void showCalculations();

        /// <summary>
        /// Method returns all N-grams smoothing algorithm smoothed probabilities
        /// </summary>
        /// <returns>N-grams smoothing algorithm smoothed probabilities</returns>
        List<Dictionary<List<int>, double[]>> getNgramProbs();
    }
}
