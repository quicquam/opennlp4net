using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.Object
{
    public class StringTokenizer
    {
        private readonly LinkedList<string> _tokens = new LinkedList<string>();
        private LinkedListNode<string> _current = null;

        public StringTokenizer(string str)
        {
            var tokens = str.Split(new[] {' ', '\n', '\t', '\r', '\f'}, StringSplitOptions.None);
            foreach (var token in tokens)
            {
                _tokens.AddLast(token);
            }
            _current = _tokens.First;
        }

        public StringTokenizer(string str, string delimiter)
        {
            Init(str, new[] {delimiter});
        }

        private void Init(string str, string[] delimiters)
        {
            var tokens = str.Split(delimiters, StringSplitOptions.None);
            foreach (var token in tokens)
            {
                _tokens.AddLast(token);
            }
            _current = _tokens.First;
        }

        public string nextToken()
        {
            var currentValue = "";
            if (_current != null)
            {
                currentValue = _current.Value;
                _current = _current.Next;
            }
            return currentValue;
        }

        public int countTokens()
        {
            return _tokens.Count;
        }

        public bool hasMoreTokens()
        {
            return _current != null;
        }
    }
}