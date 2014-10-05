using System;

namespace opennlp.tools.util.model
{
    public class ArtifactSerializers
    {
        public T Get<T>(string name) where T : ArtifactSerializer<T> 
        {
            throw new NotImplementedException();
        }

        public void Add<T>(string name, T serializer)
        {
            throw new NotImplementedException();
        }

        public void putAll(ArtifactSerializers createArtifactSerializers)
        {
            throw new NotImplementedException();
        }

        public object GetSerializerFromName(string name)
        {
            throw new NotImplementedException();
        }
    }
}
