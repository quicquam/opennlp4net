using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using j4n.IO.InputStream;
using NUnit.Framework;
using opennlp.tools.sentdetect;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class sentdetectTests
    {
        private const string ModelPath = "C:\\opennlp-models\\";
        private string _modelFilePath;
        private string _testSentence;

        [SetUp]
        public void Setup()
        {
            _modelFilePath = string.Format("{0}{1}", ModelPath, "en-sent.bin");
            var sr = new StreamReader("C:\\opennlp-models\\test-sentence.txt");
            _testSentence = sr.ReadToEnd();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void SentdetectCanLoadModelFile()
        {
            InputStream modelIn = new FileInputStream(_modelFilePath);

            try
            {
                var model = new SentenceModel(modelIn);
                var sd = new SentenceDetectorME(model);
                var sentences = sd.sentDetect(_testSentence);                
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
