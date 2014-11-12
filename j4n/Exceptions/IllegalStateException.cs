using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.Exceptions
{
    public class IllegalStateException : Exception
    {
        public IllegalStateException(string message, Exception e)
        {
        }

        public IllegalStateException(Exception e)
        {
        }

        public IllegalStateException(string message)
        {
        }
    }
}