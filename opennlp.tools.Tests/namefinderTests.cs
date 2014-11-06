using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.Writer;
using NUnit.Framework;
using opennlp.maxent;
using opennlp.maxent.io;
using opennlp.tools.namefind;
using opennlp.tools.sentdetect;
using opennlp.tools.tokenize;
using opennlp.tools.util;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class namefinderTests
    {
        private const string ModelPath = "E:\\opennlp-models\\";
        private string _modelFilePath;
        private string _testTextBlock;
        
        [SetUp]
        public void Setup()
        {
            _modelFilePath = string.Format("{0}{1}", ModelPath, "en-ner-person.bin");
            var sr = new StreamReader("E:\\opennlp-models\\test-sentence.txt");
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

                //1. convert sentence into tokens
                var modelInToken = new FileInputStream("E:\\opennlp-models\\en-token.bin");
                TokenizerModel modelToken = new TokenizerModel(modelInToken);
                Tokenizer tokenizer = new TokenizerME(modelToken);
                var tokens = tokenizer.tokenize("How far is it to the moon?");


                var nameSpans = nameFinder.find(tokens);

                //find probabilities for names
                double[] spanProbs = nameFinder.probs(nameSpans);

                //3. print names
                for (int i = 0; i < nameSpans.Length; i++)
                {
                    var s = string.Format("Span: " + nameSpans[i].ToString());
                    var c = string.Format("Covered text is: " + tokens[nameSpans[i].Start] + " " + tokens[nameSpans[i].Start + 1]);
                    var p = string.Format("Probability is: " + spanProbs[i]);
                }
                Assert.AreEqual(1, nameSpans.Count());
                Assert.AreEqual(0, nameSpans[0].Start);
                Assert.AreEqual(2, nameSpans[0].End);
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

        [Test]
        public void Test2()
        {
            			//1. convert sentence into tokens
            var modelInToken = new FileInputStream("E:\\opennlp-models\\en-token.bin");
		    	TokenizerModel modelToken = new TokenizerModel(modelInToken);

		    	Tokenizer tokenizer = new TokenizerME(modelToken);

                var tokens = new string[]
                {
                  "Why", "is", "Jack", "London", "so", "famous", "?"
                };

                ; // tokenizer.tokenize("Why is Jack London so famous?");
 
		    	//2. find names
                var modelIn = new FileInputStream("E:\\opennlp-models\\en-ner-person.bin");
		    	TokenNameFinderModel model = new TokenNameFinderModel(modelIn);
                
                NameFinderME nameFinder = new NameFinderME(model);

		    	var nameSpans = nameFinder.find(tokens);
                
                var nameGis = model.NameFinderModel as GISModel;
                if (nameGis != null)
                {
                    var modelWriter = new PlainTextGISModelWriter(nameGis, new Jfile("nameGis.txt", FileMode.Create));
                    modelWriter.persist();
                }	

		    	//find probabilities for names
		    	double[] spanProbs = nameFinder.probs(nameSpans);
		    	
		    	//3. print names
		    	for( int i = 0; i<nameSpans.Length; i++) {
		    		var s = string.Format("Span: "+nameSpans[i].ToString());
		    		var c = string.Format("Covered text is: "+tokens[nameSpans[i].Start] + " " + tokens[nameSpans[i].Start+1]);
		    		var p = string.Format("Probability is: "+spanProbs[i]);
		    	}		    	

        }

        private void DumpObject(object value, string name, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                Dumper.Dump(value, name, writer);
            }

            using (var writer = new StreamWriter(string.Format("x{0}", fileName)))
            {
                FullDumper.Write(value, 50, writer);
            }
        }
    }
}
