using System;

namespace opennlp.tools.util.model
{
    public class BaseModel
    {
        protected internal virtual void validateArtifactMap()
        {
            throw new NotImplementedException();
        }

        protected virtual internal Type DefaultFactory
        {
            get
            {
                return null;
            }
        }
    }
}
