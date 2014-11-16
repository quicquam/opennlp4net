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

    }
}
