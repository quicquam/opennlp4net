using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using j4n.IO.InputStream;
using NUnit.Framework;
using opennlp.tools.namefind;
using opennlp.tools.sentdetect;
using opennlp.tools.util;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class namefinderTests
    {
        private const string ModelPath = "C:\\opennlp-models\\";
        private string _modelFilePath;
        private string _testTextBlock;
        
        [SetUp]
        public void Setup()
        {
            _modelFilePath = string.Format("{0}{1}", ModelPath, "en-ner-person.bin");
            var sr = new StreamReader("C:\\opennlp-models\\test-sentence.txt");
            _testTextBlock = sr.ReadToEnd();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void namefinderCanGetNameArrayFromTestData()
        {
            InputStream modelIn = new FileInputStream(_modelFilePath);

            try
            {
                var model = new TokenNameFinderModel(modelIn);
                var nameFinder = new NameFinderME(model);

                var sentence = new []
                {   "Pierre",
                    "Vinken",
                    "is",
                    "61",
                    "years",
                    "old",
                    "."
                };

                var nameSpans = nameFinder.find(sentence);
                var res = "";

                foreach (Span nameSpan in nameSpans)
                {
                    res = nameSpan.ToString();
                }
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
