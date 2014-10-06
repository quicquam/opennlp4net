using System;
using System.Collections.Generic;

namespace opennlp.tools.util.model
{
    public class ArtifactSerializers
    {
        private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();

        public T Get<T>(string name) where T : class
        {
            var serializer = _dictionary[name];
            return serializer != null ? serializer as T : null;
        }

        public void Add<T>(string name, T serializer)
        {
            _dictionary.Add(name, serializer);
        }

        public Type GetValueType(string name)
        {
            var serializer = _dictionary[name];
            return serializer != null ? serializer.GetType() : null;
        }

        public object GetValueObject(string name)
        {
            var serializer = _dictionary[name];
            return serializer ?? null;
        }

        public void putAll(ArtifactSerializers createArtifactSerializers)
        {
            throw new NotImplementedException();
        }
    }
}
