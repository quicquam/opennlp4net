using System;

namespace opennlp.tools.nonjava.cmdline.Exceptions
{
    public class CmdlineParserException : Exception
    {
        public CmdlineParserException(string message)
            :base(message)
        {
            
        }
    }
}
