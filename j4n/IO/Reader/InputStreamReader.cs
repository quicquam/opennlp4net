using System.IO;
using j4n.IO.File;
using j4n.IO.InputStream;

namespace j4n.IO.Reader
{
    public class InputStreamReader : Reader
    {
        public InputStreamReader(BufferedInputStream bufferedInputStream)
            : base(bufferedInputStream)
        {
            throw new System.NotImplementedException();
        }

        public InputStreamReader(InputStream.InputStream bufferedInputStream, string encoding = null)
            : base(bufferedInputStream)
        {
            throw new System.NotImplementedException();
        }

        public InputStreamReader(FileInputStream bufferedInputStream, string encoding = null)
            : base(bufferedInputStream)
        {
            throw new System.NotImplementedException();
        }

        public InputStreamReader(Stream bufferedInputStream, string encoding)
        {
            throw new System.NotImplementedException();
        }

        public InputStreamReader(FileInputStream bufferedInputStream, Charset encoding)
        {
            throw new System.NotImplementedException();
        }

        public InputStreamReader(InputStream.InputStream bufferedInputStream, Charset encoding)
        {
            throw new System.NotImplementedException();
        }

        public InputStreamReader(Stream bufferedInputStream)
        {
            throw new System.NotImplementedException();
        }
    }
}
