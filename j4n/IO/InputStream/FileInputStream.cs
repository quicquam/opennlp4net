using j4n.IO.File;

namespace j4n.IO.InputStream
{
    public class FileInputStream : InputStream
    {
        public FileInputStream(Jfile jfile)
            :base(jfile.Name)
        {
            throw new System.NotImplementedException();
        }

        public FileInputStream(string path)
            :base(path)
        {
            throw new System.NotImplementedException();
        }
    }
}
