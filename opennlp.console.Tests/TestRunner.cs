using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using opennlp.tools.cmdline.sentdetect;

namespace opennlp.console.Tests
{
    [TestFixture]
    public class TestRunner
    {
        private const string SentenceDectectorOutFileName = "sentence.out.txt";

        [Test]
        public void SentenceDetectToolLoadsInputRunsAndCreatesOutput()
        {
            var sentenceDetectTool = new SentenceDetectorTool();
            var fileList = new List<string> { "en-sent.bin", "test-sentence.txt", SentenceDectectorOutFileName };
            sentenceDetectTool.run(fileList.ToArray());
            var lines = File.ReadAllLines(SentenceDectectorOutFileName, Encoding.Unicode);
            foreach (var line in lines)
            {
                ;
            }
        }
    }
}
