using System;
using NUnit.Framework;
using j4n.IO.InputStream;
using opennlp.tools.parser;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class parserTests
    {
        [SetUp]
        public void Setup()
        {
            _modelFilePath = string.Format("{0}{1}", ModelPath, "en-parser-chunking.bin");
        }

        [TearDown]
        public void TearDown()
        {
        }

        private const string ModelPath = "C:\\opennlp-models\\";
        private string _modelFilePath;

        [Test]
        public void TokenizeCanGetTokensArrayFromTestData()
        {
            InputStream modelIn = new FileInputStream(_modelFilePath);

            var model = new ParserModel(modelIn);
            var parser = ParserFactory.create(model);

            var sentence = "The quick brown fox jumps over the lazy dog .";
            var topParses = StandAloneParserTool.parseLine(sentence, parser, 1);
        }
    }
}

