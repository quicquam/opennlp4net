using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace opennlp.tools.Tests
{
    public class TestsBase
    {
        protected string[] GetVerificationStrings(string path)
        {
            var verificationStrings = File.ReadAllLines(path);
            return verificationStrings.ToArray();
        }

        protected string GetVerificationString(string path)
        {
            var verificationString = File.ReadAllText(path);
            return verificationString;
        }
    }
}
