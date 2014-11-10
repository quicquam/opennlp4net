using System.Collections.Generic;
using opennlp.tools.util;

namespace opennlp.tools.coref.mention
{
    public class StubDefaultParse : Parse
    {
        public StubDefaultParse(parser.Parse parse, int i)
        {
            throw new System.NotImplementedException();
        }

        public parser.Parse Parse { get; set; }
        public int CompareTo(Parse other)
        {
            throw new System.NotImplementedException();
        }

        public int SentenceNumber { get; private set; }
        public IList<Parse> NounPhrases { get; private set; }
        public IList<Parse> NamedEntities { get; private set; }
        public IList<Parse> Children { get; private set; }
        public IList<Parse> SyntacticChildren { get; private set; }
        public IList<Parse> Tokens { get; private set; }
        public string SyntacticType { get; private set; }
        public string EntityType { get; private set; }
        public bool ParentNAC { get; private set; }
        public Parse Parent { get; private set; }
        public bool NamedEntity { get; private set; }
        public bool NounPhrase { get; private set; }
        public bool Sentence { get; private set; }
        public bool CoordinatedNounPhrase { get; private set; }
        public bool Token { get; private set; }
        public int EntityId { get; private set; }
        public Span Span { get; private set; }
        public Parse PreviousToken { get; private set; }
        public Parse NextToken { get; private set; }
    }
}
