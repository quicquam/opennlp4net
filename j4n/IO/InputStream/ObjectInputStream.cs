using System;

namespace j4n.IO.InputStream
{
    public class ObjectInputStream : InputStream
    {
        public ObjectInputStream(string path)
            :base(path)
        {
            
        }

// ReSharper disable InconsistentNaming
        public double readDouble()
        {
            throw new NotImplementedException();
        }

        public int readInt()
        {
            throw new NotImplementedException();
        }

        public string readUTF()
        {
            throw new NotImplementedException();
        }
    }
}
