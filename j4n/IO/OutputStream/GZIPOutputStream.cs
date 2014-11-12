using System;

namespace j4n.IO.OutputStream
{
    public class GZIPOutputStream : OutputStream
    {
        public GZIPOutputStream(FileOutputStream fileOutputStream)
            : base(fileOutputStream.InnerStream)
        {
            throw new NotImplementedException();
        }
    }
}