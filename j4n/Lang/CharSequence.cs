using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.Lang
{
    public class CharSequence
    {
        private string _innerstring;

        public CharSequence(string s)
        {
            _innerstring = s;
        }

        public int length()
        {
            return _innerstring.Length;
        }

        public CharSequence subSequence(int start, int end)
        {
            return new CharSequence(_innerstring.Substring(start, (end - start)));
        }

        public char charAt(int index)
        {
            return _innerstring[index];
        }

        public override string ToString()
        {
            return _innerstring;
        }
    }
}
