using System;
using System.Collections.Generic;
using j4n.IO.File;
using j4n.IO.InputStream;
using opennlp.tools.sentdetect;

namespace opennlp.tools.util.model
{
    public class BaseModel
    {
        public string Language { get; set; }

        protected internal readonly IDictionary<string, object> artifactMap = new Dictionary<string, object>();

        protected BaseModel(string componentName, string languageCode, IDictionary<string, string> manifestInfoEntries, BaseToolFactory factory)
        {
            throw new NotImplementedException();
        }

        protected BaseModel(string componentName, InputStream languageCode)
        {
            throw new NotImplementedException();
        }

        protected BaseModel(string componentName, Jfile languageCode)
        {
            throw new NotImplementedException();
        }

        protected BaseModel(string componentName, Uri languageCode)
        {
            throw new NotImplementedException();
        }

        protected internal virtual void validateArtifactMap()
        {
            throw new NotImplementedException();
        }

        protected internal virtual void checkArtifactMap()
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
