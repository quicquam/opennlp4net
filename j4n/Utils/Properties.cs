using System;
using System.IO;
using System.Linq;
using System.Text;
using j4n.Interfaces;
using j4n.IO.InputStream;
using System.Collections.Generic;
using j4n.IO.OutputStream;

namespace j4n.Utils
{
    public class Properties : Dictionary<string, string>
    {
        public void load(InputStream @in)
        {
            var lines = ReadLines(@in.InnerStream);
            foreach (var line in lines)
            {
                if (!line.StartsWith("#"))
                {
                    var parts = line.Split('=').ToList();
                    if (parts.Count == 2)
                    {
                        Add(parts[0], parts[1]);
                    }
                }
            }
        }

        public void store(OutputStream @out, string empty)
        {
            throw new System.NotImplementedException();
        }

        public void setProperty(string key, string value)
        {
            Add(key, value);
        }

        public IEnumerable<KeyValuePair<object, object>> entrySet()
        {
            throw new System.NotImplementedException();
        }

        public string getProperty(string key)
        {
            string val;
            TryGetValue(key, out val);
            return val;
        }

        public string getProperty(string key, string defaultValue)
        {
            string val;
            if (!TryGetValue(key, out val))
            {
                val = defaultValue;
            }
            return val;
        }

        public IEnumerable<string> ReadLines(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}