using System;
using System.IO;
using j4n.Security;
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
            SetAbsolutePath(fileName);
        }

        private void SetAbsolutePath(string fileName)
        {
            AbsolutePath = Path.IsPathRooted(fileName) ? fileName : string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), fileName);
        }

        public string Name { get; private set; }
        public bool IsFile { get; private set; }
        public bool IsDirectory { get; set; }
        public string AbsolutePath { get; set; }
        public Jfile AbsoluteFile { get; set; }
        public Jfile ParentFile { get; set; }
        private bool _deleteOnExit = false;

        public static Jfile createTempFile(string events, object o)
        {
           return new Jfile(Path.GetTempPath() + events + ".tmp");
        }

        ~Jfile()
        {
            if (_deleteOnExit)
            {
            }
            //    System.IO.File.Delete(Name);
        }

        public void deleteOnExit()
        {
            _deleteOnExit = true;
        }

        public void delete()
        {
            
        }

        public bool exists()
        {
            return System.IO.File.Exists(Name);
        }

        public bool canRead()
        {
            return new UserFileAccessRights(Name).canRead();
        }

        public bool canWrite()
        {
            return new UserFileAccessRights(Name).canWrite();
        }

        public Jfile[] listFiles(FileFilter fileFilter = null)
        {
            throw new NotImplementedException();
        }
    }
}