using System;
using j4n.Interfaces;

namespace j4n.IO.Writer
{
    public class Writer : Closeable, Flushable
    {
        public void close()
        {
            throw new NotImplementedException();
        }

        public void flush()
        {
            throw new NotImplementedException();
        }

        public void write(string toLine)
        {
            throw new NotImplementedException();
        }

        public void write(char toLine)
        {
            throw new NotImplementedException();
        }
    }
}
