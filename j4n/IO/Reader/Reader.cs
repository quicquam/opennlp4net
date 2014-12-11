using System;
using System.IO;
using System.Security.Cryptography;
using j4n.Interfaces;

namespace j4n.IO.Reader
{
    public class Reader : Closeable
    {
        protected StreamReader StreamReader;
        
        public Reader(InputStream.InputStream stream)
        {
            StreamReader = new StreamReader(stream.GetStream());
        }

        protected Reader(Reader reader)
        {
            StreamReader = reader.StreamReader;
        }

        public Reader()
        {
        }

        public void close()
        {
            
        }

        public int read()
        {
            throw new NotImplementedException();
        }

        public int read(char[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}