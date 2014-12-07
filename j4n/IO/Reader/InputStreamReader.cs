using System.IO;
using j4n.IO.File;
using j4n.IO.InputStream;

namespace j4n.IO.Reader
{
    public class InputStreamReader : Reader
    {
        private string Encoding;
        public InputStreamReader(InputStream.InputStream stream)
            : base(stream)
        {
            Encoding = "UTF-8";
        }

        public InputStreamReader(InputStream.InputStream stream, string encoding = null)
            : base(stream)
        {
            Encoding = encoding;
        }

        public InputStreamReader(FileInputStream stream, string encoding = null)
            : base(stream)
        {
            Encoding = encoding;
        }

        public InputStreamReader(Stream stream, string encoding)
        {
            throw new System.NotImplementedException();
        }

        public InputStreamReader(FileInputStream stream, Charset encoding)
        {
            throw new System.NotImplementedException();
        }

        public InputStreamReader(InputStream.InputStream stream, Charset encoding)
        {
            throw new System.NotImplementedException();
        }

        public InputStreamReader(Stream stream)
        {
            StreamReader = new StreamReader(stream);
        }
    }
}
