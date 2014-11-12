using System;
using System.IO;
using j4n.IO.InputStream;
using NUnit.Framework;
using opennlp.tools.namefind;
using opennlp.tools.postag;
using opennlp.tools.tokenize;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class postaggerTests
    {
        private const string ModelPath = "C:\\opennlp-models\\";
        private string _modelFilePath;
        private string _testTextBlock;

        [SetUp]
        public void Setup()
        {
            _modelFilePath = string.Format("{0}{1}", ModelPath, "en-pos-maxent.bin");
            var sr = new StreamReader("C:\\opennlp-models\\test-sentence.txt");
            _testTextBlock = sr.ReadToEnd();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void postaggerCanGetTagArrayFromTestData()
        {
            InputStream modelIn = new FileInputStream(_modelFilePath);

            try
            {
                var model = new POSModel(modelIn);
                var tagger = new POSTaggerME(model);

                var sent = new[]
                {
                    "Most", "large", "cities", "in", "the", "US", "had",
                    "morning", "and", "afternoon", "newspapers", "."
                };
                var tags = tagger.tag(sent);
                var probs = tagger.probs();
                var topSequences = tagger.topKSequences(sent);
            }
            catch (IOException e)
            {
                var s = e.StackTrace;
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