using System;
using System.IO;
using j4n.IO.File;

namespace j4n.IO.Writer
{
    public class FileWriter : OutputStreamWriter
    {
        public FileWriter(Jfile jfile)
            : base(jfile.FileStream)
        {
        }

        public FileWriter(string filename)
            : base(System.IO.File.Open(filename, FileMode.Create))
        {
            
        }
    }
}
