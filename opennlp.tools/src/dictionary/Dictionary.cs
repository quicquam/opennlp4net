using System.Collections.Generic;
using j4n.Interfaces;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.IO.Reader;
using opennlp.tools.util;
using opennlp.tools.util.model;

namespace opennlp.tools.dictionary
{
    public class Dictionary
    {
        public Dictionary(InputStream fileInputStream)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary(bool fileInputStream)
        {
            throw new System.NotImplementedException();
        }

        public decimal MaxTokenCount { get; set; }

        public HashSet<string> asStringSet()
        {
            throw new System.NotImplementedException();
        }

        public void serialize(OutputStream @out)
        {
            throw new System.NotImplementedException();
        }

        public bool contains(StringList entryForSearch)
        {
            throw new System.NotImplementedException();
        }

        public void put(StringList stringList)
        {
            throw new System.NotImplementedException();
        }

        public static Dictionary parseOneEntryPerLine(InputStreamReader @in)
        {
            throw new System.NotImplementedException();
        }
    }
}