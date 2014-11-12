using System;
using System.IO;
using j4n.Interfaces;

namespace j4n.IO.Writer
{
    public class Writer : Closeable, Flushable
    {
        public Stream InnerStream;

        public void close()
        {
            InnerStream.Close();
        }

        public void flush()
        {
            InnerStream.Flush();
        }

        public void write(string str)
        {
            var bytes = GetBytes(str);
            InnerStream.Write(bytes, 0, bytes.GetLength(0));
        }

        public void write(char charValue)
        {
            var bytes = BitConverter.GetBytes(charValue);
            InnerStream.Write(bytes, 0, bytes.GetLength(0));
        }

        private byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length*sizeof (char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}