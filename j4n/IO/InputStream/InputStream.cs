using System.IO;
using j4n.Interfaces;

namespace j4n.IO.InputStream
{
    public class InputStream : Closeable
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
    }
}
