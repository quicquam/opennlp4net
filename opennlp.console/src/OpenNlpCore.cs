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
                        if (!string.IsNullOrEmpty(_options.InputFilename))
                        {
                            //InitializeInputStream();
                        }
                        if (!string.IsNullOrEmpty(_options.OutputFilename))
                        {
                            //InitializeOutputStream();
                        }
                    }
                }
            }
        }

        private string[] CreateCommandLineArguments()
        {
            var argList = new List<string> {_options.ModelName};
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

        private void InitializeOutputStream()
        {
            try
            {
                _outputStream = new StreamWriter(_options.OutputFilename);
            }
            catch (Exception ex)
            {

            }
        }

        private void InitializeInputStream()
        {
            try
            {
                _inputStream = new StreamReader(_options.InputFilename);
            }
            catch (Exception ex)
            {
                
            }
        }

        public void Process()
        {

        }
    }
}
