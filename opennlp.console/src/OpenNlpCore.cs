using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using opennlp.console.cmdline;

namespace opennlp.console
{
    public class OpenNlpCore
    {
        private Assembly _assembly;
        
        private readonly Options _options;
        private StreamReader _inputStream = null;
        private StreamWriter _outputStream = null;

        private BasicCmdLineTool _cmdLineTool;

        public OpenNlpCore(Options options)
        {
            _options = options;
            _assembly = Assembly.GetExecutingAssembly();
            if (_assembly != null && !string.IsNullOrEmpty(_options.ToolName))
            {
                var type = GetToolType();
                if (type != null)
                {
                    _cmdLineTool = Activator.CreateInstance(type) as BasicCmdLineTool;
                    if (_cmdLineTool != null)
                    {
                        _cmdLineTool.run(CreateCommandLineArguments());
                    }
                }
            }
        }

        private string[] CreateCommandLineArguments()
        {
            var argList = new List<string> {_options.ModelName};

            if(!string.IsNullOrEmpty(_options.InputFilename))
                argList.Add(_options.InputFilename);

            if (!string.IsNullOrEmpty(_options.OutputFilename))
                argList.Add(_options.OutputFilename);
            return argList.ToArray();
        }

        private Type GetToolType()
        {
            var toolName = string.Format("{0}Tool", _options.ToolName);
            return (from t in _assembly.GetTypes()
                    where t.IsClass
                    && (t.Name == toolName)
                    select t).FirstOrDefault();
        }

        public void Process()
        {

        }
    }
}
