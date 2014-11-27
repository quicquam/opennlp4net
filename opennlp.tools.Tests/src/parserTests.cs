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
        public void TokenizeCanGetTokensArrayFromTestData()
        {
            InputStream modelIn = new FileInputStream(_modelFilePath);

            var model = new ParserModel(modelIn);
            var parser = ParserFactory.create(model);

            const string sentence = "The quick brown fox jumps over the lazy dog .";
            const string correctParse = "(TOP (NP (NP (DT The) (JJ quick) (JJ brown) (NN fox) (NNS jumps)) (PP (IN over) (NP (DT the) (JJ lazy) (NN dog))) (. .)))";
            var sb = new StringBuilder();
            var topParse = StandAloneParserTool.parseLine(sentence, parser, 1).FirstOrDefault();
            if (topParse != null)
            {
                topParse.show(sb);
            }
            Assert.AreEqual(correctParse, sb.ToString());

            modelIn.close();
        }
    }
}

