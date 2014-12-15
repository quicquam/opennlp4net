using j4n.Interfaces;
using System.IO;

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
    }
}