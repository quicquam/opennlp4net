using System;
using System.Collections.Generic;
using opennlp.tools.sentdetect;

namespace opennlp.tools.util.model
{
    public class BaseModel
    {
        protected BaseModel(string componentName, string languageCode, IDictionary<string, string> manifestInfoEntries, SentenceDetectorFactory sdFactory)
        {
            throw new NotImplementedException();
        }

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
