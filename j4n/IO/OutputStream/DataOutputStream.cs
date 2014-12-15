using System;
using System.IO;
using System.Text;
using j4n.Interfaces;

namespace j4n.IO.OutputStream
{
    public class DataOutputStream : OutputStream, Closeable
    {
        public DataOutputStream(OutputStream os)
            : base(os.InnerStream)
        {
        }

        public DataOutputStream()
            : base(new FileStream("outputstream.out", FileMode.Create))
        {
        }

        public void writeShort(short s)
        {
            byte[] bytes = BitConverter.GetBytes(s);
            Array.Reverse(bytes);
            InnerStream.Write(bytes, 0, bytes.GetLength(0));
        }

        public void writeUTF(string s)
        {
            var length = (short)s.Length;
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
    }
}