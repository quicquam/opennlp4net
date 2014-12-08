using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using opennlp.tools.cmdline;

namespace opennlp.console
{
    public class OpenNlpCore
    {
        private const string OpenNlpToolsAssemblyName = "opennlp.tools.dll";
        
        private readonly Options _options;
        private BasicCmdLineTool _cmdLineTool;

        public OpenNlpCore(Options options)
        {
            _options = options;
            var assembly = Assembly.LoadFrom(CreateAssembyPath());
            if (assembly != null && !string.IsNullOrEmpty(_options.ToolName))
            {
                var type = GetToolType(assembly);
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

        private static string CreateAssembyPath()
        {
            return string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), OpenNlpToolsAssemblyName);
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

        private Type GetToolType(Assembly assembly)
        {
            var toolName = string.Format("{0}Tool", _options.ToolName);
            return (from t in assembly.GetTypes()
                    where t.IsClass
                    && (t.Name == toolName)
                    select t).FirstOrDefault();
        }

        public void Process()
        {

        }
    }
}
