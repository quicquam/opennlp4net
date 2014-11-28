using System.IO;
using j4n.IO.InputStream;
using NUnit.Framework;
using opennlp.tools.chunker;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class chunkerTests
    {
        [SetUp]
        public void Setup()
        {
            _sent = new[]
            {
                "Rockwell", "International", "Corp.", "'s",
                "Tulsa", "unit", "said", "it", "signed", "a", "tentative", "agreement",
                "extending", "its", "contract", "with", "Boeing", "Co.", "to",
                "provide", "structural", "parts", "for", "Boeing", "'s", "747",
                "jetliners", "."
            };

            _pos = new[]
            {
                "NNP", "NNP", "NNP", "POS", "NNP", "NN",
                "VBD", "PRP", "VBD", "DT", "JJ", "NN", "VBG", "PRP$", "NN", "IN",
                "NNP", "NNP", "TO", "VB", "JJ", "NNS", "IN", "NNP", "POS", "CD", "NNS",
                "."
            };

            _modelFilePath = string.Format("{0}{1}", ModelPath, "en-chunker.bin");
        }

        [TearDown]
        public void TearDown()
        {
        }

        private const string ModelPath = @"..\..\models\";
        private const string DataPath = @"..\..\data\";
        private string _modelFilePath;
        private string[] _sent;
        private string[] _pos;

        [Test]
        public void ChunkerCanGetTokensArrayFromTestData()
        {
            InputStream modelIn = new FileInputStream(_modelFilePath);

            try
            {
                var model = new ChunkerModel(modelIn);
                var chunker = new ChunkerME(model);
                var tags = chunker.chunk(_sent, _pos);
                var probs = chunker.probs();
            }
            catch (IOException e)
            {
                string s = e.StackTrace;
            }
            finally
            {
                try
                {
                    modelIn.close();
                }
                catch (IOException e)
                {
                }
            }
        }
    }
}