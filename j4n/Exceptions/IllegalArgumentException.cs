using System;

namespace j4n.Exceptions
{
    public class IllegalArgumentException : Exception
    {
        public IllegalArgumentException(string message)
           : base(message)
        {
            
        }
    }
}
