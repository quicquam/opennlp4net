using j4n.IO.File;

namespace j4n.IO.InputStream
{
    public class FileInputStream : InputStream
    {
        public FileInputStream(Jfile jfile)
            : base(jfile.Name)
        {
        }

        public FileInputStream(string path)
            : base(path)
        {
        }

        public InputStream Channel { get; set; }
    }
}