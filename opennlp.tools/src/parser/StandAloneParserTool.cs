using System.Collections.Generic;
using System.Text;
using j4n.Lang;
using j4n.Object;
using opennlp.tools.util;

namespace opennlp.tools.parser
{
    public static class StandAloneParserTool
    {
        private static Pattern untokenizedParentPattern1 = Pattern.compile("([^ ])([({)}])");
        private static Pattern untokenizedParentPattern2 = Pattern.compile("([({)}])([^ ])");

        public static Parse[] parseLine(string line, Parser parser, int numParses)
        {
            //line = untokenizedParentPattern1.matcher(line).replaceAll("$1 $2");
            //line = untokenizedParentPattern2.matcher(line).replaceAll("$1 $2");
            StringTokenizer str = new StringTokenizer(line);
            StringBuilder sb = new StringBuilder();
            IList<string> tokens = new List<string>();
            while (str.hasMoreTokens())
            {
                string tok = str.nextToken();
                tokens.Add(tok);
                sb.Append(tok).Append(" ");
            }
            string text = sb.ToString().Substring(0, sb.Length - 1);
            Parse p = new Parse(text, new Span(0, text.Length), AbstractBottomUpParser.INC_NODE, 0, 0);
            int start = 0;
            int i = 0;
            for (IEnumerator<string> ti = tokens.GetEnumerator(); ti.MoveNext(); i++)
            {
                string tok = ti.Current;
                p.insert(new Parse(text, new Span(start, start + tok.Length), AbstractBottomUpParser.TOK_NODE, 0, i));
                start += tok.Length + 1;
            }
            Parse[] parses;
            if (numParses == 1)
            {
                parses = new Parse[] { parser.parse(p) };
            }
            else
            {
                parses = parser.parse(p, numParses);
            }
            return parses;
        }
    }
}
