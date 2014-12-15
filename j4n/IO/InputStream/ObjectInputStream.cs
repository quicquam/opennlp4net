using System;

namespace j4n.IO.InputStream
{
    public class ObjectInputStream : DataInputStream
    {
        public ObjectInputStream(string path)
            : base(path)
        {
        }
    }
}