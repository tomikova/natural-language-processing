using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Master.Utility {

    /// <summary>
    /// Class modeling model training options
    /// Author: Tomislav
    /// </summary>
    class TrainOptions {

        [Option('t', "train", Required = true)]
        public string TrainFile { get; set; }

        [Option('s', "smoother", Required = true)]
        public string Smoother { get; set; }

        [Option('o', "order", Required = false, DefaultValue = 3)]
        public int Order { get; set; }

        [Option('m', "model", Required = false, DefaultValue = "model.xml")]
        public string ModelFile { get; set; }

        [HelpOption]
        public string GetUsage() {
            var usage = new StringBuilder();
            usage.AppendLine("TRAINING MODEL:");
            usage.AppendLine("Option: (-t|--train) training_file, input file used for training, required");
            usage.AppendLine("Usage example: -t C:/Corpus/training_corpus.txt");
            usage.AppendLine("Option: (-s|--smoother) smoother_name, smoother that will be used for training, required");
            usage.AppendLine("Supported smoothers: addone, wittenbell, absdiscount, simplekn, modkn");
            usage.AppendLine("Usage example: -s wittenbell");
            usage.AppendLine("Option: (-o|--order) ngram_order, ngram order, not required, default = 3");
            usage.AppendLine("Usage example: -o 2");
            usage.AppendLine("Option: (-m|--model) model_path, path where model will be saved including model name, "
                +"not required, default = model.xml");
            usage.AppendLine("Usage example: -m C:/Model/NewModel.xml");
            return usage.ToString();
        }
    }

    /// <summary>
    /// Class modeling model evaluation options
    /// Author: Tomislav
    /// </summary>
    class ModelEvaluateOptions {
        [Option('m', "model", Required = true)]
        public string ModelFile { get; set; }

        [Option('t', "test", Required = true)]
        public string TestFile { get; set; }

        [HelpOption]
        public string GetUsage() {
            var usage = new StringBuilder();
            usage.AppendLine("EVALUATING MODEL:");
            usage.AppendLine("Option: (-m|--model)  model_path, path to the model file, required");
            usage.AppendLine("Usage example: -m C:/Model/model.xml");
            usage.AppendLine("Option: (-t|--test)  test_file_path, path to the test file, required");
            usage.AppendLine("Usage example: -t C:/Tests/test.txt");
            return usage.ToString();
        }
    }

    /// <summary>
    /// Class modeling sentence evaluation options
    /// Author: Tomislav
    /// </summary>
    class SentenceEvaluateOption {
        [Option('m', "model", Required = true)]
        public string ModelFile { get; set; }

        [Option('s', "sentence", Required = true)]
        public string TestSentence { get; set; }

        [HelpOption]
        public string GetUsage() {
            var usage = new StringBuilder();
            usage.AppendLine("EVALUATING SENTENCE:");
            usage.AppendLine("Option: (-m|--model)  model_path, path to the model file, required");
            usage.AppendLine("Usage example: -m C:/Model/model.xml");
            usage.AppendLine("Option: (-s|--sentence)  sentence, sentence to be evaluated, required");
            usage.AppendLine("Usage example: -s \"This sentence will be evaluated\"");
            return usage.ToString();
        }
    }

    /// <summary>
    /// Class modeling help options
    /// Author: Tomislav
    /// </summary>
    class HelpOption {
        [Option('h', "help")]
        public bool Help { get; set; }

        [HelpOption]
        public string GetUsage() {
            var usage = new StringBuilder();
            usage.AppendLine("HELP:");
            usage.AppendLine("Option: (-h|--help)");
            return usage.ToString();
        }
    }
}
