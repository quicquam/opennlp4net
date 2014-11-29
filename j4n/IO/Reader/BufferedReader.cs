using System;
using System.IO;
using System.Runtime.InteropServices;

namespace j4n.IO.Reader
{
    public class BufferedReader : Reader
    {
        public BufferedReader(Reader inputStreamReader) : base(inputStreamReader)
        {
        }

        public BufferedReader(FileReader inputStreamReader)
        {
            throw new NotImplementedException();
        }

        public BufferedReader(StringReader inputStreamReader)
        {
            throw new NotImplementedException();
        }

        public string readLine()
        {
            throw new NotImplementedException();
        }

        public void reset()
        {
            throw new NotImplementedException();
        }
    }
}