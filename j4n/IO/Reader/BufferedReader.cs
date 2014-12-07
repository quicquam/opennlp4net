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
        private byte[] _buffer;
        
        public BufferedReader(Reader inputStreamReader, int bufferSize = 2048)
            : base(inputStreamReader)
        {
            _bufferSize = bufferSize;
            _buffer = new byte[_bufferSize];
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
            return StreamReader.ReadLine();
        }

        public void reset()
        {
            throw new NotImplementedException();
        }
    }
}