using System;
using System.IO;
using System.Text;

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

        public OutputStream(TextWriter stream)
        {
            throw new NotImplementedException();
        }

        private void writeShort(short s)
        {
            byte[] bytes = BitConverter.GetBytes(s);
            Array.Reverse(bytes);
            InnerStream.Write(bytes, 0, bytes.GetLength(0));
        }

        public void writeUTF(string s)
        {
            var length = (short) s.Length;
            writeShort(length);
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            InnerStream.Write(bytes, 0, bytes.GetLength(0));
        }

        public void writeInt(int i)
        {
            byte[] bytes = BitConverter.GetBytes(i);
            Array.Reverse(bytes);
            InnerStream.Write(bytes, 0, bytes.GetLength(0));
        }

        public void writeDouble(double d)
        {
            byte[] bytes = BitConverter.GetBytes(d);
            Array.Reverse(bytes);
            InnerStream.Write(bytes, 0, bytes.GetLength(0));
        }

        public void flush()
        {
            InnerStream.Flush();
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