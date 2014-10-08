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
    }
}
