using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.IO.OutputStream
{
    public class ByteArrayOutputStream : OutputStream
    {
        public ByteArrayOutputStream(Stream stream)
            : base(stream)
        {
        }

        public byte[] toByteArray()
        {
            throw new NotImplementedException();
        }

        public sbyte[] toSbyteArray()
        {
            throw new NotImplementedException();
        }

        public void write(byte[] buffer, int i, int length)
        {
            throw new NotImplementedException();
        }
    }
}