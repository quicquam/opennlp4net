using System;

namespace j4n.IO.Reader
{
    public class BufferedReader : Reader
    {
        public BufferedReader(Reader inputStreamReader) : base(inputStreamReader)
        {
            throw new NotImplementedException();
        }

        public BufferedReader(FileReader inputStreamReader)
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