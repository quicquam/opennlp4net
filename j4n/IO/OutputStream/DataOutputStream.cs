using System;
using j4n.Interfaces;

namespace j4n.IO.OutputStream
{
    public class DataOutputStream : Interfaces.OutputStream, Closeable
    {
// ReSharper disable InconsistentNaming

        public DataOutputStream(Interfaces.OutputStream os)
        {
            
        }

        public DataOutputStream()
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
