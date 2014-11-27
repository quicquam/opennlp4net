using j4n.Object;
using opennlp.tools.util;
using System.Collections.Generic;
using System.Text;

namespace opennlp.tools.parser
{
    public static class StandAloneParserTool
    {

        public static Parse[] parseLine(string line, Parser parser, int numParses)
        {
            var str = new StringTokenizer(line);
            var sb = new StringBuilder();
            var tokens = new List<string>();
            while (str.hasMoreTokens())
            {
                var tok = str.nextToken();
                tokens.Add(tok);
                sb.Append(tok).Append(" ");
            }
            var text = sb.ToString().Substring(0, sb.Length - 1);
            var p = new Parse(text, new Span(0, text.Length), AbstractBottomUpParser.INC_NODE, 0, 0);
            var start = 0;
            var i = 0;
            for (var ti = tokens.GetEnumerator(); ti.MoveNext(); i++)
            {
                var tok = ti.Current;
                if (tok == null) continue;
                p.insert(new Parse(text, new Span(start, start + tok.Length), AbstractBottomUpParser.TOK_NODE, 0, i));
                start += tok.Length + 1;
            }
            return numParses == 1 ? new [] { parser.parse(p) } : parser.parse(p, numParses);
        }
    }
}
