using System;

namespace j4n.Exceptions
{
    public class NumberFormatException : Exception
    {
        public NumberFormatException(string message)
            : base(message)
        {
        }
    }
}