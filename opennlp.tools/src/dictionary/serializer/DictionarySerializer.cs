using System.Collections.Generic;
using j4n.Interfaces;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using opennlp.tools.ngram;
using opennlp.tools.tokenize;

namespace opennlp.tools.dictionary.serializer
{
    public class DictionarySerializer
    {
        public static void serialize(OutputStream @out, IEnumerator<Entry> entries, bool b)
        {
            throw new System.NotImplementedException();
        }

        public static void create(InputStream @in, DetokenizationDictionary.EntryInserterAnonymousInnerClassHelper entryInserterAnonymousInnerClassHelper)
        {
            throw new System.NotImplementedException();
        }

        public static void create(InputStream @in, NGramModel.EntryInserterAnonymousInnerClassHelper entryInserterAnonymousInnerClassHelper)
        {
            throw new System.NotImplementedException();
        }
    }
}
