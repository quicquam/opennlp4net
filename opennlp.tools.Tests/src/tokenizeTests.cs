using System.IO;
using System.Linq;
using j4n.IO.InputStream;
using NUnit.Framework;
using opennlp.tools.tokenize;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class tokenizeTests
    {
        private const string ModelPath = @"..\..\models\";
        private const string DataPath = @"..\..\data\";
        private string _modelFilePath;
        private string _testTextBlock;

        [SetUp]
        public void Setup()
        {
            _modelFilePath = string.Format("{0}{1}", ModelPath, "en-token.bin");
            var sr = new StreamReader(string.Format("{0}{1}", DataPath, "test-sentence.txt"));
            _testTextBlock = sr.ReadToEnd();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TokenizeCanGetTokensArrayFromTestData()
        {
            InputStream modelIn = new FileInputStream(_modelFilePath);

            try
            {
                var model = new TokenizerModel(modelIn);
                var tokenizer = new TokenizerME(model);
                var tokens = tokenizer.tokenize(_testTextBlock);
                Assert.AreEqual(tokens.Count(), 217);
            }
            catch (IOException e)
            {
                var s = e.StackTrace;
            }
            finally
            {
                if (modelIn != null)
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
}