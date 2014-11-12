using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace j4n.IO.File
{
    public class Jfile
    {
        public SafeFileHandle FileHandle;
        public FileStream FileStream;
        public static string separator;

        public Jfile(string fileName, FileMode filemode = FileMode.Open)
        {
            Name = fileName;
            FileStream = System.IO.File.Open(fileName, filemode);
        }

        public string Name { get; private set; }
        public bool IsFile { get; private set; }
        public bool IsDirectory { get; set; }
        public string AbsolutePath { get; set; }
        public Jfile AbsoluteFile { get; set; }
        public Jfile ParentFile { get; set; }

        public static Jfile createTempFile(string events, object o)
        {
            throw new NotImplementedException();
        }

        public void deleteOnExit()
        {
            throw new NotImplementedException();
        }

        public void delete()
        {
            throw new NotImplementedException();
        }

        public bool exists()
        {
            throw new NotImplementedException();
        }

        public bool canRead()
        {
            throw new NotImplementedException();
        }

        public bool canWrite()
        {
            throw new NotImplementedException();
        }
    }
}