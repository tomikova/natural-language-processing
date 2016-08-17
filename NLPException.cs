using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master.Utility {

    /// <summary>
    /// Class of exception that can occur in NLP application
    /// Author: Tomislav
    /// </summary>
    class NLPException : Exception {

        /// <summary>
        /// Default constructor
        /// </summary>
        public NLPException() {
        }

        /// <summary>
        /// Constructor with one parameter
        /// </summary>
        /// <param name="message">Exception message</param>
        public NLPException(string message)
            : base(message) {
        }

        /// <summary>
        /// Constructor with two parameters
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public NLPException(string message, Exception inner)
            : base(message, inner) {
        }
    }
}
