using System;
using System.IO;
using System.Text;

namespace j4n.IO.InputStream
{
    public class DataInputStream : InputStream
    {
        public DataInputStream(BufferedInputStream bufferedInputStream)
            : base(bufferedInputStream)
        {
            throw new NotImplementedException();
        }

        public DataInputStream(InputStream bufferedInputStream)
            : base(bufferedInputStream)
        {
        }

        public DataInputStream(GZIPInputStream bufferedInputStream)
            : base(bufferedInputStream)
        {
            throw new NotImplementedException();
        }

        protected DataInputStream(string path)
            : base(path)
        {
        }

        public short readShort()
        {
            var bytes = new byte[2];
            InnerStream.Read(bytes, 0, 2);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public int readInt()
        {
            var bytes = new byte[4];
            InnerStream.Read(bytes, 0, 4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public long readLong()
        {
            var bytes = new byte[8];
            InnerStream.Read(bytes, 0, 8);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        public double readDouble()
        {
            var bytes = new byte[8];
            InnerStream.Read(bytes, 0, 8);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        public byte readByte()
        {
            return (byte)InnerStream.ReadByte();
        }

        public byte PeekByte()
        {
            long offset = InnerStream.Position;
            var byteValue = (byte)InnerStream.ReadByte();
            InnerStream.Seek(offset, SeekOrigin.Begin);
            return byteValue;
        }

        public string readUTF()
        {
            int val = readShort();

            var buffer = new byte[val];
            if (InnerStream.Read(buffer, 0, val) < 0)
            {
                throw new IOException("EOF");
            }
            return Encoding.ASCII.GetString(buffer);
        }

        public static UInt64 ReverseBytes(UInt64 value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }
    }
}