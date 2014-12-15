using System;

namespace opennlp.tools.util
{
    public abstract class ObjectStream<T>
    {
        public virtual void reset()
        {
        }

        public virtual void close()
        {
        }

        public abstract T read();
    }
}