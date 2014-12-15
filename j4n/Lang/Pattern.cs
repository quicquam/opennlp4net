using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace j4n.Lang
{
    public class Pattern
    {
        public readonly Regex _regex;

        public static Pattern compile(string s)
        {
            return new Pattern(s);
        }

        public Pattern(string s)
        {
            _regex = new Regex(s);
        }

        public Matcher matcher(string input)
        {
            return new Matcher(this, input);
        }

        public string pattern()
        {
            return _regex.ToString();
        }

        public Matcher matcher(StringBuilder sentenceString)
        {
            return matcher(sentenceString.ToString());
        }

        public static Pattern compile(string input, TextCase textCase)
        {
            return new Pattern(input);
        }

        public enum TextCase { CASE_INSENSITIVE, CASE_SENSITIVE };

        public string[] Split(string lexemeStr)
        {
            throw new NotImplementedException();
        }
    }
}