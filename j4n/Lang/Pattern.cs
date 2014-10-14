﻿using System;
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
    }
}
