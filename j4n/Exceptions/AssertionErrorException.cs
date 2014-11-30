using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.Exceptions
{
    public class AssertionErrorException : Exception
    {
        public AssertionErrorException(string message)
            :base(message)
        {
            
        }
    }
}
