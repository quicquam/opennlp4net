using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.IO.InputStream
{
    public class UncloseableInputStream : InputStream
    {
        public UncloseableInputStream(string path) : base(path)
        {
        }

        public UncloseableInputStream(Stream stream) : base(stream)
        {
        }

        protected UncloseableInputStream(InputStream fileInputStream) : base(fileInputStream)
        {
        }
    }
}
