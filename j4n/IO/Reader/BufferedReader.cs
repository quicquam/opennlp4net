using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace j4n.IO.Reader
{
    public class BufferedReader : Reader
    {
        private readonly int _bufferSize = 2048;
        private int _readOffset = 0;
        private int _bufferOffset = 0;
        private string _stringBuffer;
        
        public BufferedReader(Reader inputStreamReader, int bufferSize = 2048)
            : base(inputStreamReader)
        {
            _bufferSize = bufferSize;
            var buffer = new char[_bufferSize];
            int charsRead = StreamReader.ReadBlock(buffer, 0, _bufferSize);
            _stringBuffer = new string(buffer, 0, charsRead);
        }

        public BufferedReader(FileReader inputStreamReader, int bufferSize = 2048)
        {
            throw new NotImplementedException();
        }

        public BufferedReader(StringReader inputStreamReader, int bufferSize = 2048)
        {
            throw new NotImplementedException();
        }

        public string readLine()
        {
            var line = new StringReader(_stringBuffer.Substring(_bufferOffset)).ReadLine();
            if (line != null)
            {
                _bufferOffset += line.Length +1;
            }
            return line;
        }

        public void reset()
        {
            _bufferOffset = 0;
        }
    }
}