using System;

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

// ReSharper disable InconsistentNaming
        public double readDouble()
        {
            throw new NotImplementedException();
        }

        public int readInt()
        {
            throw new NotImplementedException();
        }

        public string readUTF()
        {
            throw new NotImplementedException();
        }
    }
}
