using System;
using j4n.Interfaces;

namespace j4n.IO.Reader
{
    public class Reader : Closeable
    {
        public Reader(InputStream.InputStream bufferedInputStream)
        {
            throw new System.NotImplementedException();
        }

        protected Reader(Reader bufferedInputStream)
        {
            throw new NotImplementedException();
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
    }
}