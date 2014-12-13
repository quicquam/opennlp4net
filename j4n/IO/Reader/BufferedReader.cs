using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace j4n.IO.Reader
{
    public class BufferedReader : Reader
    {        
        public BufferedReader(Reader inputStreamReader, int bufferSize = 2048)
            : base(inputStreamReader)
        {
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
    }
}