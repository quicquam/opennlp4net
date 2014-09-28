using System;

namespace j4n.IO.File
{
    public class Jfile
    {
        public Jfile(string s)
        {
            throw new NotImplementedException();
        }

        public string Name { get; set; }
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
