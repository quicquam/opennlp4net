using System;
using System.Collections.Generic;

namespace opennlp.tools.util.model
{
    public class ArtifactSerializers
    {
        private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();

        public bool Contains(string name)
        {
            return _dictionary.ContainsKey(name);
        }

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
            if (!_dictionary.ContainsKey(name)) return null;
            var serializer = _dictionary[name];
            return serializer != null ? serializer.GetType() : null;
        }

        public object GetValueObject(string name)
        {
            if (!_dictionary.ContainsKey(name)) return null;
            var serializer = _dictionary[name];
            return serializer;
        }

        public void putAll(ArtifactSerializers createArtifactSerializers)
        {
            throw new NotImplementedException();
        }
    }
}