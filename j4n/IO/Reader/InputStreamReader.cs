using System.IO;
using j4n.IO.File;
using j4n.IO.InputStream;

namespace j4n.IO.Reader
{
    public class InputStreamReader : Reader
    {
        private Charset _charset;
        public InputStreamReader(InputStream.InputStream stream)
            : base(stream)
        {
            _charset = Charset.forName("UTF-8");
        }

        public InputStreamReader(InputStream.InputStream stream, string encoding = null)
            : base(stream)
        {
            _charset = !string.IsNullOrEmpty(encoding) ? Charset.forName(encoding) : Charset.defaultCharset();
        }

        public InputStreamReader(FileInputStream stream, string encoding = null)
            : base(stream)
        {
            _charset = !string.IsNullOrEmpty(encoding) ? Charset.forName(encoding) : Charset.defaultCharset();
        }

        public InputStreamReader(Stream stream, string encoding)
        {
            throw new System.NotImplementedException();
        }

        public InputStreamReader(FileInputStream stream, Charset charset)
            :base(stream)
        {
            _charset = charset;
        }

        public InputStreamReader(InputStream.InputStream stream, Charset charset)
            : base(stream)
        {
            _charset = charset;
        }

        public InputStreamReader(Stream stream)
        {
            StreamReader = new StreamReader(stream);
        }
    }
}
