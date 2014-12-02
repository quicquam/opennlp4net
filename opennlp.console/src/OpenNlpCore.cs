using System;
using System.IO;

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
