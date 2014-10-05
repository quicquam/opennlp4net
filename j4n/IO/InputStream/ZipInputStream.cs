using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using j4n.Utils;

namespace j4n.IO.InputStream
{
    public class ZipInputStream : InputStream
    {
        public ZipInputStream(InputStream @in)
            :base(@in)
        {
            throw new NotImplementedException();
        }

        public ZipEntry NextEntry { get; set; }

        public void closeEntry()
        {
            throw new NotImplementedException();
        }
    }
}
