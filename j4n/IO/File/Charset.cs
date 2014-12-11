using System.Text;

namespace j4n.IO.File
{
    public class Charset
    {
        private readonly Encoding _encoding;

        public Charset(Encoding encoding)
        {
            _encoding = encoding;
        }

        public string name()
        {
            return _encoding.EncodingName;
        }

        public static Charset forName(string name)
        {
            Encoding encoding;
            switch (name)
            {
                case "UTF-8":
                encoding = new UTF8Encoding();
                    break;
                case "ASCII":
                    encoding = new ASCIIEncoding();
                    break;
                default:
                    encoding = new UnicodeEncoding();
                    break;
            }
            return new Charset(encoding);
        }

        public static Charset defaultCharset()
        {
            return new Charset(new UnicodeEncoding());
        }
    }
}