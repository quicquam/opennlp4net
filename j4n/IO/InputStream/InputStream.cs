using System;
using System.IO;
using System.Text;
using j4n.Interfaces;

namespace j4n.IO.InputStream
{
    public class InputStream : Closeable
    {
        public readonly string Path;
        public readonly Stream InnerStream;

        public Stream GetStream()
        {
            return InnerStream;
        }

        public InputStream(string path)
        {
            Path = path;
            InnerStream = new FileStream(path, FileMode.Open);
        }

        public InputStream(Stream stream)
        {
            InnerStream = stream;
        }

        protected InputStream(InputStream stream)
        {
            InnerStream = stream.InnerStream;
        }

        public void close()
        {
            InnerStream.Close();
        }

        public int read(sbyte[] buffer, int i, int length)
        {
            throw new System.NotImplementedException();
        }

        public void Flush()
        {
            InnerStream.Flush();
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return InnerStream.Seek(offset, origin);
        }

        public void SetLength(long value)
        {
            InnerStream.SetLength(value);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return InnerStream.Read(buffer, offset, count);
        }

        public bool CanRead
        {
            get { return InnerStream.CanRead; }
        }

        public bool CanSeek
        {
            get { return InnerStream.CanSeek; }
        }

        public long Length
        {
            get { return InnerStream.Length; }
        }

        public long Position
        {
            get { return InnerStream.Position; }

            set { InnerStream.Position = value; }
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
            var e = BitConverter.IsLittleEndian;
            var bytes = new byte[8];
            InnerStream.Read(bytes, 0, 8);
            var v = BitConverter.ToUInt64(bytes, 0);
            var ul = ReverseBytes(v);
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
            return (byte) InnerStream.ReadByte();
        }

        public byte PeekByte()
        {
            long offset = InnerStream.Position;
            var byteValue = (byte) InnerStream.ReadByte();
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