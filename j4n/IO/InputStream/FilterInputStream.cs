using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using j4n.Interfaces;

namespace j4n.IO.InputStream
{
    public class FilterInputStream : Closeable
    {
        protected FilterInputStream(InputStream @in)
        {
            throw new NotImplementedException();
        }

        public virtual void close()
        {
            throw new NotImplementedException();
        }
    }
}
