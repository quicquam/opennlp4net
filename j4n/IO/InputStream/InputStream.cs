using System.IO;
using j4n.Interfaces;

namespace j4n.IO.InputStream
{
    public class InputStream : Stream, Closeable
    {
        public readonly string Path;
        protected readonly Stream File;
        public InputStream(string path)
        {
            Path = path;
            File = new FileStream(path, FileMode.Open);
        }

        public InputStream(Stream stream)
        {
            Path = "stdin";
            File = stream;
        }

        protected InputStream(InputStream fileInputStream)
        {
            throw new System.NotImplementedException();
        }

        public void close()
        {
            File.Close();
        }

        public int read(sbyte[] buffer, int i, int length)
        {
            throw new System.NotImplementedException();
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanRead
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool CanSeek
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool CanWrite
        {
            get { throw new System.NotImplementedException(); }
        }

        public override long Length
        {
            get { throw new System.NotImplementedException(); }
        }

        public override long Position { get; set; }
    }
}
