using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.Logging
{
    public class Logger
    {
        public static Logger getLogger(string name)
        {
            throw new NotImplementedException();
        }

        public bool isLoggable(object warning)
        {
            throw new NotImplementedException();
        }

        public enum Level { INFO, WARNING , ERROR}

        public void warning(string s)
        {
            throw new NotImplementedException();
        }
    }
}
