using System;
using System.IO;
using j4n.Interfaces;

namespace j4n.IO.Reader
{
    public class Reader : Closeable
    {
        public InputStream.InputStream InnerStream;
        public Reader(InputStream.InputStream stream)
        {
            InnerStream = stream;
        }

        protected Reader(Reader reader)
        {
            InnerStream = reader.InnerStream;
        }

        public Reader()
        {
        }

        public void close()
        {
            throw new NotImplementedException();
        }

        public int read()
        {
            throw new NotImplementedException();
        }

        public int read(char[] buffer, int p1, int p2)
        {
            throw new NotImplementedException();
        }
    }
}