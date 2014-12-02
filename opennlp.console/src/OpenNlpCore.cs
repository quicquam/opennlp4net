using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace opennlp.console
{
    public class OpenNlpCore
    {
        private readonly Options _options;
        private StreamReader _inputStream = null;
        private StreamWriter _outputStream = null;

        public OpenNlpCore(Options options)
        {
            _options = options;
            if (!string.IsNullOrEmpty(_options.InputFilename))
            {
                InitializeInputStream();
            }
            if (!string.IsNullOrEmpty(_options.OutputFilename))
            {
                InitializeOutputStream();
            }
        }

        private Type GetToolType()
        {
            return (from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass
                    && (t.Name == _options.ToolName 
                    || t.Name == string.Format("{0}Tool",_options.ToolName))
                    select t).First();
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
