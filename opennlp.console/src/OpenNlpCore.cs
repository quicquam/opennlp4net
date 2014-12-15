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
        private Dictionary<string, string> _parameters; 

        private readonly BasicCmdLineTool _cmdLineTool;

        public OpenNlpCore(Dictionary<string, string> parameters)
        {
            _parameters = parameters;
            var assembly = Assembly.LoadFrom(CreateAssembyPath());
            if (assembly != null)
            {
                var type = GetToolType(assembly);
                if (type != null)
                {
                    _cmdLineTool = Activator.CreateInstance(type) as BasicCmdLineTool;
                }
            }
        }

        private static string CreateAssembyPath()
        {
            return string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), OpenNlpToolsAssemblyName);
        }

        private string[] CreateCommandLineArguments()
        {
            var argList = new List<string>();
            if (!string.IsNullOrEmpty(GetParameter("model")))
                argList.Add(GetParameter("model"));

            if (!string.IsNullOrEmpty(GetParameter("input")))
                argList.Add(GetParameter("input"));

            if (!string.IsNullOrEmpty(GetParameter("output")))
                argList.Add(GetParameter("output"));
            return argList.ToArray();
        }

        private Type GetToolType(Assembly assembly)
        {
            var toolName = string.Format("{0}Tool", GetParameter("toolName"));
            return (from t in assembly.GetTypes()
                    where t.IsClass
                    && (t.Name == toolName)
                    select t).FirstOrDefault();
        }

        private string GetParameter(string key)
        {
            return _parameters.ContainsKey(key) ? _parameters[key] : "";
        }

        public void Process()
        {
            if (_cmdLineTool != null)
            {
                _cmdLineTool.run(CreateCommandLineArguments());
            }
        }
    }
}
