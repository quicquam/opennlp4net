using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.IO.OutputStream
{
    public class BufferedOutputStream : OutputStream
    {
        public BufferedOutputStream(FileOutputStream fileOutputStream, int ioBufferSize)
            : base(fileOutputStream.InnerStream)
        {
        }
    }
}