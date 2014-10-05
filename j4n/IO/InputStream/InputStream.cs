using System.IO;
using j4n.Interfaces;

namespace j4n.IO.InputStream
{
    public class InputStream : Closeable
    {
        public readonly string Path;
        protected readonly Stream Stream;
        public InputStream(string path)
        {
            Path = path;
            Stream = new FileStream(path, FileMode.Open);
        }

        public InputStream(Stream stream)
        {
            Path = "stdin";
            Stream = stream;
        }

        protected InputStream(InputStream stream)
        {
            throw new System.NotImplementedException();
        }

        public void close()
        {
            Stream.Close();
        }

        public int read(sbyte[] buffer, int i, int length)
        {
            throw new System.NotImplementedException();
        }

        public void Flush()
        {
            Stream.Flush();
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return Stream.Seek(offset, origin);
        }

        public void SetLength(long value)
        {
            Stream.SetLength(value);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return Stream.Read(buffer, offset, count);
        }

     /*   public void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
        */
        public bool CanRead
        {
            get { return Stream.CanRead; }
        }

        public bool CanSeek
        {
            get { return Stream.CanSeek; }
        }

/*        public override bool CanWrite
        {
            get { throw new System.NotImplementedException(); }
        }
        */
        public long Length
        {
            get { return Stream.Length; }
        }

        public long Position
        {
            get
            {
                return Stream.Position;
            }

            set
            {
                Stream.Position = value;
            }
        }
    }
}
