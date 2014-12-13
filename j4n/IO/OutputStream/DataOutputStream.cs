using System;
using System.IO;
using j4n.Interfaces;

namespace j4n.IO.OutputStream
{
    public class DataOutputStream : OutputStream, Closeable
    {
        public DataOutputStream(OutputStream os)
            : base(os.InnerStream)
        {
        }

        public DataOutputStream()
            : base(new FileStream("outputstream.out", FileMode.Create))
        {
        }
    }
}