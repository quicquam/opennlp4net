using System.Collections.Generic;
using j4n.IO.InputStream;
using NUnit.Framework;
using opennlp.tools.parser;
using System.Linq;
using System.Text;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class parserTests
    {
        private const string ModelPath = @"..\..\models\";
        private const string DataPath = @"..\..\data\";
        private string _modelFilePath;
        private string _testTextBlock;
        
        [SetUp]
        public void Setup()
        {
            _modelFilePath = string.Format("{0}{1}", ModelPath, "en-parser-chunking.bin");
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void ParserCanGetParsesArrayFromTestData()
        {
            InputStream modelIn = new FileInputStream(_modelFilePath);

            var model = new ParserModel(modelIn);
            var parser = ParserFactory.create(model);

            const string sentence = "The quick brown fox jumps over the lazy dog .";
            var parseStrings = new List<string>();
            var sb = new StringBuilder();
            var parses = StandAloneParserTool.parseLine(sentence, parser, 5)
                .OrderBy(y => y.TagSequenceProb)
                .ToList();
            foreach (var parse in parses)
            {
                parse.show(sb);
                parseStrings.Add(sb.ToString());
                sb.Clear();
            }

            modelIn.close();
        }
    }
}

