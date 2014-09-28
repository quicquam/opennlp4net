using System;
using System.Collections.Generic;

namespace opennlp.tools.util
{
    public abstract class BaseToolFactory
    {
        public virtual void validateArtifactMap()
        {
            throw new NotImplementedException();
        }

        public virtual IDictionary<string, object> createArtifactMap()
        {
            throw new NotImplementedException();
        }

        public virtual IDictionary<string, string> createManifestEntries()
        {
            throw new NotImplementedException();
        }
    }
}
