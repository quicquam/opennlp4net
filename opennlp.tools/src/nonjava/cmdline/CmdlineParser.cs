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

        public CmdlineParser()
        {
            OutputFileName = string.Empty;
            InputFileName = string.Empty;
            ToolName = string.Empty;
        }

        public Dictionary<string, object> Parse(string[] args)
        {
            _cmdLineConstants = new CmdLineConstants();
            var parameters = new Dictionary<string, object>();
            
            FindToolName(args, parameters);
            FindModelNameByPosition(args, parameters);
            FindInputFileName(args, parameters);
            FindOutputFileName(args, parameters);
            FindToolParameters(args, parameters);

            return parameters;
        }

        private void FindToolName(string[] args, Dictionary<string, object> dictionary)
        {
            var index = Array.IndexOf(_cmdLineConstants.ToolNames, args[0]);
            if (index != -1)
            {
                dictionary.Add("tool", _cmdLineConstants.ToolNames[index]);
            }
            index = Array.IndexOf(_cmdLineConstants.ToolNames, string.Format("{0}Tool", args[0]));
            if (index != -1)
            {
                dictionary.Add("tool", string.Format("{0}Tool", _cmdLineConstants.ToolNames[index]));
            }
        }

        private void FindModelNameByPosition(string[] args, Dictionary<string, object> dictionary)
        {
            if (args.Count() > 1 && args[1].EndsWith(".bin"))
            {
                dictionary.Add("model", args[1]);
            }
        }

        private void FindOutputFileName(string[] args, Dictionary<string, object> dictionary)
        {
            var outputFileName = FindPipedFile(">", args);
            if (!string.IsNullOrEmpty(outputFileName))
            {
                dictionary.Add("output", outputFileName);
            }          
        }

        private void FindInputFileName(string[] args, Dictionary<string, object> dictionary)
        {
            var inputFileName = FindPipedFile("<", args);
            if (!string.IsNullOrEmpty(inputFileName))
            {
                dictionary.Add("input", inputFileName);
            }            
        }

        private void FindToolParameters(string[] args, Dictionary<string, object> dictionary)
        {
            var parameterList = new List<KeyValuePair<string, object>>();
            for (var i = 1; i < args.Length; i++)
            {
                var s = args[i];
                if (s.StartsWith("-") && i + 1 < args.Length)
                {
                    var key = s.Substring(1);
                    object val;
                    if (IsBinaryOption(key))
                    {
                        val = true;
                    }
                    else
                    {
                        val = args[i + 1];                        
                    }
                    parameterList.Add(new KeyValuePair<string, object>(key, val));
                }
            }
            if (parameterList.Any())
            {
                foreach (var keyValuePair in parameterList)
                {
                    dictionary.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        private bool IsBinaryOption(string key)
        {
            return key == "alphaNumOpt";
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
