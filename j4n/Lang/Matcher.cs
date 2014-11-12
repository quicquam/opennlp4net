using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace j4n.Lang
{
    public class Matcher
    {
        private string _input;
        private Pattern _pattern;

        public Matcher(string input)
        {
            _input = input;
        }

        public Matcher(Pattern pattern, string input)
        {
            _pattern = pattern;
            _input = input;
        }

        public bool find()
        {
            return _pattern.ToString() == _input;
        }

        public bool matches()
        {
            return _pattern._regex.IsMatch(_input);
        }

        public string group(int i)
        {
            var m = _pattern._regex.Match(_input);
            var g = m.Groups[i];
            return g.Value;
        }

        public int? start()
        {
            throw new NotImplementedException();
        }

        public int? end()
        {
            throw new NotImplementedException();
        }
    }
}