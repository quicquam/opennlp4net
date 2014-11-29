using CommandLine;

namespace opennlp.console
{
    public class Options
    {
        [Option('t', "toolName", Required = true, HelpText = "the OpenNLP Tool to be invoked.")]
        public string ToolName { get; set; }

        [Option('i', "input", Required = false, HelpText = "the (source) input data.")]
        public string InputFilename { get; set; }

        [Option('o', "output", Required = false, HelpText = "the output data.")]
        public string OutputFilename { get; set; }
        
        [Option('a', "abbDict", Required = false, HelpText = "abbreviation dictionary in XML format.")]
        public string Abbreviation { get; set; }

        [Option('p', "params", Required = false, HelpText = "training parameters file.")]
        public string Parameters { get; set; }

        [Option('r', "iterations", Required = false, HelpText = "number of training iterations, ignored if -params is used.")]
        public int Iterations { get; set; }

        [Option('c', "cutoff", Required = false, HelpText = "minimal number of times a feature must be seen, ignored if -params is used.")]
        public int Cutoff { get; set; }

        [Option('m', "model", Required = false, HelpText = "the model file.")]
        public string ModelName { get; set; }

        [Option('l', "lang", Required = false, HelpText = "language which is being processed.")]
        public string ProcessingLanguage { get; set; }

        [Option('d', "data", Required = false, HelpText = "data to be used, usually a file name.")]
        public string DataParameter { get; set; }

        [Option('e', "encoding", Required = false, HelpText = "encoding for reading and writing text, if absent the system default is used.")]
        public string EncodingName { get; set; }

        // Options used by subset of tools
        [Option('r', "resources", Required = false, HelpText = "The resources directory.")] // NameFinder & POSTagger
        public string ResourcesDirectory { get; set; }

        [Option('f', "featuregen", Required = false, HelpText = "The feature generator descriptor file.")] // NameFinder
        public string FeatureGen { get; set; }

        [Option('y', "type", Required = false, HelpText = "The type of the token name finder model.")] // NameFinder & POSTagger
        public string Type { get; set; }

        [Option('n', "formatname", Required = false, HelpText = "data format, might have its own parameters.")] // POSTagger
        public string FormatName { get; set; }

        [Option('x', "dictionarypath", Required = false, HelpText = "The XML tag dictionary file path.")] // POSTagger
        public string DictionaryPath { get; set; }

        [Option('g', "ngram", Required = false, HelpText = "NGram cutoff. If not specified will not create ngram dictionary.")] // Parser
        public string NgramCutoff { get; set; }

        [Option('h', "headrulesfile", Required = false, HelpText = "head rules file.")] // Parser
        public string HeadRulesFile { get; set; }

        [Option('s', "parsertype", Required = false, HelpText = "one of CHUNKING or TREEINSERT, default is CHUNKING.")] // Parser
        public string ParserType { get; set; }

    }
}
