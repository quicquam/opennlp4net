using System;
using System.IO;
using j4n.IO.File;

namespace j4n.IO.OutputStream
{
    public class FileOutputStream : OutputStream
    {
        public FileOutputStream(Jfile file)
            : base(new FileStream(file.AbsolutePath, FileMode.Create))
        {
        }

        public FileOutputStream(string file)
            : base(new FileStream(file, FileMode.Create))
        {
        }
    }
}