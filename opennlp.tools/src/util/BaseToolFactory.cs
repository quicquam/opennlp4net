using System;
using System.Collections.Generic;
using opennlp.model;
using opennlp.tools.util.model;

namespace opennlp.tools.util
{
    public abstract class BaseToolFactory
    {
        protected internal ArtifactProvider artifactProvider;

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

        public static BaseToolFactory create(Type factoryClass, BaseModel<AbstractModel> p1)
        {
            throw new NotImplementedException();
        }
    }
}
