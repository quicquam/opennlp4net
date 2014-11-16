using System;

namespace j4n.Exceptions
{
    public class NumberFormatException : Exception
    {
        private string p;

        public NumberFormatException(string message)
            : base(message)
        {
        }
    }
}