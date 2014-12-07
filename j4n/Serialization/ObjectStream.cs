using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.Serialization
{
    public abstract class ObjectStream<T>
    {
        public virtual void reset()
        {
            throw new NotImplementedException();
        }

        public virtual void close()
        {
            throw new NotImplementedException();
        }

        public abstract T read();
    }
}