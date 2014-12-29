using System;
using opennlp.tools.nonjava.cmdline;

namespace opennlp.console
{
    class Program
    {
        static void Main(string[] args)
        {
            var cmd = new CmdlineParser();
            var parameters = cmd.Parse(args);
            if (parameters != null)
            {
                // consume Options instance properties
                if (!parameters.ContainsKey("tool"))
                {
                    Console.WriteLine("No tool specified");
                }
                else
                {
                    var opennlpCore = new OpenNlpCore(parameters);
                    opennlpCore.Process();
                }
            }
        }
    }
}
