using System;
using System.IO;
using j4n.IO.OutputStream;

namespace j4n.IO.Writer
{
    public class OutputStreamWriter : Writer
    {
        public OutputStreamWriter(Stream os)
        {
            InnerStream = os;
        }

        public OutputStreamWriter(OutputStream.OutputStream os)
        {
            InnerStream = os.InnerStream;
        }

        public OutputStreamWriter(FileOutputStream os, string encoding)
        {
            InnerStream = os.InnerStream;
        }
    }
}