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
        private CmdLineConstants _cmdLineConstants;

        public Dictionary<string, object> Parse(string[] args)
        {
            _cmdLineConstants = new CmdLineConstants();
            var parameters = new Dictionary<string, object>();
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
            var inputFileName = FindInputFileName(args);
            if (!string.IsNullOrEmpty(inputFileName.Key))
            {
                parameters.Add(inputFileName.Key, inputFileName.Value);
            }
            var outputFileName = FindOutputFileName(args);
            if (!string.IsNullOrEmpty(outputFileName.Key))
            {
                parameters.Add(outputFileName.Key, outputFileName.Value);
            }

            var parameterList = FindToolParameters(args);
            if (parameterList.Any())
            {
                foreach (var keyValuePair in parameterList)
                {
                    parameters.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return parameters;
        }

        private KeyValuePair<string, object> FindToolName(string[] args)
        {
            var index = Array.IndexOf<string>(_cmdLineConstants.ToolNames, args[0]);
            if (index != -1)
            {
                return new KeyValuePair<string, object>("toolName", _cmdLineConstants.ToolNames[index]);
            }
            index = Array.IndexOf<string>(_cmdLineConstants.ToolNames, string.Format("{0}Tool", args[0]));
            if (index != -1)
            {
                return new KeyValuePair<string, object>("toolName", string.Format("{0}Tool", _cmdLineConstants.ToolNames[index]));
            }
            return new KeyValuePair<string, object>();
        }

        private KeyValuePair<string, object> FindModelName(string[] args)
        {
            var index = Array.IndexOf<string>(args, "-model");
            if (index != -1 && index+1 < args.GetUpperBound(0))
            {
                return new KeyValuePair<string, object>("model", args[index + 1]);
            }
            foreach (var s in args.Where(s => s.EndsWith(".bin")))
            {
                return new KeyValuePair<string, object>("model", s);
            }
            return new KeyValuePair<string, object>();
        }

        private KeyValuePair<string, object> FindOutputFileName(string[] args)
        {
            var inputFileName = FindPipedFile(">", args);
            if (!string.IsNullOrEmpty(inputFileName))
            {
                return new KeyValuePair<string, object>("input", inputFileName);
            }
            return new KeyValuePair<string, object>();
        }

        private KeyValuePair<string, object> FindInputFileName(string[] args)
        {
            var outputFileName = FindPipedFile("<", args);
            if (!string.IsNullOrEmpty(outputFileName))
            {
                return new KeyValuePair<string, object>("output", outputFileName);
            }
            return new KeyValuePair<string, object>();
        }

        private List<KeyValuePair<string, object>> FindToolParameters(string[] args)
        {
            var parameterList = new List<KeyValuePair<string, object>>();
            for (var i = 1; i < args.Length; i++)
            {
                var s = args[i];
                if (s.StartsWith("-") && i + 1 < args.Length)
                {
                    var key = s.Substring(1);
                    var val = args[i + 1];
                    parameterList.Add(new KeyValuePair<string, object>(key, val));
                }
            }
            return parameterList;
        }

        private string FindPipedFile(string redirector, string[] args)
        {
            var filename = "";
            for(var i = 1; i < args.Length; i++)
            {
                var s = args[i];
                if (s.Contains(redirector))
                {
                    if (s.EndsWith(redirector) && i+1 < args.Length)
                    {
                        filename = args[i + 1];
                    }
                    else
                    {
                        filename = s.Substring(1);
                    }
                }
            }
            return filename;
        }
    }
}
