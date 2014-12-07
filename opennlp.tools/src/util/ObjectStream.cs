using System;

namespace opennlp.tools.util
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