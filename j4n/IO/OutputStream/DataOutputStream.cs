using System;
using System.IO;
using j4n.Interfaces;

namespace j4n.IO.OutputStream
{
    public class DataOutputStream : OutputStream, Closeable
    {
// ReSharper disable InconsistentNaming

        public DataOutputStream(OutputStream os)
            : base(os.InnerStream)
        {
            
        }

        public DataOutputStream() 
            : base(new FileStream("outputstream.out", FileMode.Create))
        {
            
        }

        public void writeUTF(string s)
        {
            throw new NotImplementedException();
        }

        public void writeInt(int i)
        {
            throw new NotImplementedException();
        }

        public void writeDouble(double d)
        {
            throw new NotImplementedException();
        }

        public void flush()
        {
            throw new NotImplementedException();
        }
    }
}
