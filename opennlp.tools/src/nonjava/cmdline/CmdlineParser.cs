using System;
using System.Collections.Generic;

namespace opennlp.tools.nonjava.cmdline
{
    public class CmdlineParser
    {
        public string ToolName { get; private set; }
        public string InputFileName { get; private set; }
        public string OutputFileName { get; private set; }
        
        public Dictionary<string, string> Parse(string[] args)
        {
            return new Dictionary<string, string>();
        }

        private KeyValuePair<string, string> FindToolName(string[] args)
        {
            throw new NotImplementedException();
        }

        private KeyValuePair<string, string> FindModelName(string[] args)
        {
            throw  new NotImplementedException();
        }

        private KeyValuePair<string, string> FindOutputFileName(string[] args)
        {
            throw new NotImplementedException();
        }

        private KeyValuePair<string, string> FindInputFileName(string[] args)
        {
            throw new NotImplementedException();
        }

        private List<KeyValuePair<string, string>> FindToolParameters(string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
