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

        [SetUp]
        public void Setup()
        {
            _modelFilePath = string.Format("{0}{1}", ModelPath, "en-sent.bin");
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
            }
            catch (IOException e)
            {
                var s = e.StackTrace.ToString();
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
