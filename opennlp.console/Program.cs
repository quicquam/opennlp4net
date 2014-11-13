﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opennlp.console
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // consume Options instance properties
                if (string.IsNullOrEmpty(options.ToolName))
                {
                    Console.WriteLine("No toolName specified");
                }
                else
                {
                    var opennlpCore = new OpenNlpCore(options);
                    opennlpCore.Process();
                }
            }
        }
    }
}
