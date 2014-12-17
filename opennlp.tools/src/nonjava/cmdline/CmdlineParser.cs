using System;
using System.Collections.Generic;
using System.Linq;
using opennlp.tools.cmdline;

namespace opennlp.tools.nonjava.cmdline
{
    public class CmdlineParser
    {
        public string ToolName { get; private set; }
        public string InputFileName { get; private set; }
        public string OutputFileName { get; private set; }
        
        public Dictionary<string, string> Parse(string[] args)
        {
            var parameters = new Dictionary<string, string>();
            var toolName = FindToolName(args);
            if (!string.IsNullOrEmpty(toolName.Key))
            {
                parameters.Add(toolName.Key, toolName.Value);
            }
            var modelName = FindModelName(args);
            if (!string.IsNullOrEmpty(modelName.Key))
            {
                parameters.Add(modelName.Key, modelName.Value);
            }
            return parameters;
        }

        private KeyValuePair<string, string> FindToolName(string[] args)
        {
            var names = new ToolNames();
            var index = Array.IndexOf<string>(names.NameStrings, args[0]);
            if (index != -1)
            {
                return new KeyValuePair<string, string>("toolName", names.NameStrings[index]);
            }
            index = Array.IndexOf<string>(names.NameStrings, string.Format("{0}Tool",args[0]));
            if (index != -1)
            {
                return new KeyValuePair<string, string>("toolName", string.Format("{0}Tool", names.NameStrings[index]));
            }
            return new KeyValuePair<string, string>();
        }

        private KeyValuePair<string, string> FindModelName(string[] args)
        {
            var index = Array.IndexOf<string>(args, "-model");
            if (index != -1 && index+1 < args.GetUpperBound(0))
            {
                return new KeyValuePair<string, string>("model", args[index+1]);
            }
            foreach (var s in args.Where(s => s.EndsWith(".bin")))
            {
                return new KeyValuePair<string, string>("model", s);
            }
            return new KeyValuePair<string, string>();
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
