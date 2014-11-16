using System;
using System.IO;

namespace j4n.IO.OutputStream
{
    public class OutputStream
    {
        public Stream InnerStream;

        public OutputStream(OutputStream os)
        {
            InnerStream = os.InnerStream;
        }

        public OutputStream(Stream stream)
        {
            InnerStream = stream;
        }

        public void close()
        {
            InnerStream.Close();
        }

        public void write(sbyte[] classBytes)
        {
            throw new NotImplementedException();
        }
    }
}