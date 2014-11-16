using System;

namespace opennlp.tools.util
{
    public class InvalidFormatException : Exception
    {
        public InvalidFormatException(string message, Exception exception)
            :base(message, exception)
        {
            
        }
        
        public InvalidFormatException(string message)
            :base(message)
        {
        }
    }
}