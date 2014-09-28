using System;
using System.IO;
using System.Text;

namespace j4n.IO.InputStream.Native
{
    public class ByteInputStream
    {
        private readonly FileStream _file;

        public ByteInputStream(string path)
        {
            _file = new FileStream(path, FileMode.Open);
        }

        public short NextShort()
        {
            var bytes = new byte[2];
            _file.Read(bytes, 0, 2);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public int NextInt()
        {
            var bytes = new byte[4];
            _file.Read(bytes, 0, 4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public long NextLong()
        {
            var e = BitConverter.IsLittleEndian;
            var bytes = new byte[8];
            _file.Read(bytes, 0, 8);
            var v = BitConverter.ToUInt64(bytes, 0);
            var ul = ReverseBytes(v);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        public double NextDouble()
        {
            var bytes = new byte[8];
            _file.Read(bytes, 0, 8);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        public byte NextByte()
        {
            return (byte) _file.ReadByte();
        }

        public byte PeekByte()
        {
            long offset = _file.Position;
            var byteValue = (byte) _file.ReadByte();
            _file.Seek(offset, SeekOrigin.Begin);
            return byteValue;
        }

        public string NextString()
        {
            int val = NextShort();

            var buffer = new byte[val];
            if (_file.Read(buffer, 0, val) < 0)
            {
                throw new IOException("EOF");
            }
            return Encoding.Default.GetString(buffer);
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