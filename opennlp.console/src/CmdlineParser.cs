using System;
using System.Collections.Generic;

namespace opennlp.console
{
    public class CmdlineParser
    {
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
