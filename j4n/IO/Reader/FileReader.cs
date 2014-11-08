using System;
using j4n.IO.File;
using j4n.IO.InputStream;

namespace j4n.IO.Reader
{
    public class FileReader : InputStreamReader
    {
        public FileReader(string fileName)
            :base(new FileInputStream(fileName))
        {
        }

        public FileReader(Jfile file)
            :base(new FileInputStream(file.Name))
        {
        }
    }
}
