using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using j4n.IO.InputStream;
using NUnit.Framework;
using opennlp.tools.chunker;
using opennlp.tools.namefind;
using opennlp.tools.parser;
using opennlp.tools.postag;
using opennlp.tools.sentdetect;
using opennlp.tools.tokenize;
using opennlp.tools.util;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class ApiTests
    {
        private const string ModelPath = @"..\..\data\models\";
        private const string InputPath = @"..\..\data\input\";

        [Test]
        public void ChunkerCanGetTokensArrayFromTestData()
        {
            var sent = new[]
            {
                "Rockwell", "International", "Corp.", "'s",
                "Tulsa", "unit", "said", "it", "signed", "a", "tentative", "agreement",
                "extending", "its", "contract", "with", "Boeing", "Co.", "to",
                "provide", "structural", "parts", "for", "Boeing", "'s", "747",
                "jetliners", "."
            };

            var pos = new[]
            {
                "NNP", "NNP", "NNP", "POS", "NNP", "NN",
                "VBD", "PRP", "VBD", "DT", "JJ", "NN", "VBG", "PRP$", "NN", "IN",
                "NNP", "NNP", "TO", "VB", "JJ", "NNS", "IN", "NNP", "POS", "CD", "NNS",
                "."
            };

            string modelFilePath = string.Format("{0}{1}", ModelPath, "en-chunker.bin");


            InputStream modelIn = new FileInputStream(modelFilePath);

            try
            {
                var model = new ChunkerModel(modelIn);
                var chunker = new ChunkerME(model);
                string[] tags = chunker.chunk(sent, pos);
                double[] probs = chunker.probs();
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

        [Test]
        public void NamefinderLocationCanGetNameArrayFromTestData()
        {
            string nameFinderModelFilePath = string.Format("{0}{1}", ModelPath, "en-ner-location.bin");
            string tokenModelPath = string.Format("{0}{1}", ModelPath, "en-token.bin");

            InputStream modelIn = new FileInputStream(nameFinderModelFilePath);

            var model = new TokenNameFinderModel(modelIn);
            var nameFinder = new NameFinderME(model);

            //1. convert sentence into tokens
            var modelInToken = new FileInputStream(tokenModelPath);
            var modelToken = new TokenizerModel(modelInToken);
            Tokenizer tokenizer = new TokenizerME(modelToken);
            string testTextBlock;
            using (var sr = new StreamReader(string.Format("{0}{1}", InputPath, "en-ner-location.in.txt")))
            {
                testTextBlock = sr.ReadToEnd();
            }
            string[] tokens = tokenizer.tokenize(testTextBlock);

            Span[] nameSpans = nameFinder.find(tokens);

            //find probabilities for names
            double[] spanProbs = nameFinder.probs(nameSpans);

            //3. print names
            var nameList = new List<string>();
            foreach (Span t in nameSpans)
            {
                string name = "";
                for (int j = t.Start; j < t.End; j++)
                {
                    name += tokens[j] + " ";
                }

                name = name.TrimEnd(new[] {' '});
                nameList.Add(name);
            }
            Assert.AreEqual(5, nameSpans.Count());

            modelInToken.close();
            modelIn.close();
        }

        [Test]
        public void NamefinderPersonCanGetNameArrayFromTestData()
        {
            string nameFinderModelFilePath = string.Format("{0}{1}", ModelPath, "en-ner-person.bin");
            string tokenModelPath = string.Format("{0}{1}", ModelPath, "en-token.bin");

            InputStream modelIn = new FileInputStream(nameFinderModelFilePath);

            var model = new TokenNameFinderModel(modelIn);
            var nameFinder = new NameFinderME(model);

            //1. convert sentence into tokens
            var modelInToken = new FileInputStream(tokenModelPath);
            var modelToken = new TokenizerModel(modelInToken);
            Tokenizer tokenizer = new TokenizerME(modelToken);
            string testTextBlock;
            using (var sr = new StreamReader(string.Format("{0}{1}", InputPath, "en-ner-person.in.txt")))
            {
                testTextBlock = sr.ReadToEnd();
            }
            string[] tokens = tokenizer.tokenize(testTextBlock);

            Span[] nameSpans = nameFinder.find(tokens);

            //find probabilities for names
            double[] spanProbs = nameFinder.probs(nameSpans);

            //3. print names
            var nameList = new List<string>();
            foreach (Span t in nameSpans)
            {
                string name = "";
                for (int j = t.Start; j < t.End; j++)
                {
                    name += tokens[j] + " ";
                }

                name = name.TrimEnd(new[] {' '});
                nameList.Add(name);
            }
            Assert.AreEqual(4, nameSpans.Count());

            modelInToken.close();
            modelIn.close();
        }

        [Test]
        public void ParserCanGetParsesArrayFromTestData()
        {
            string modelFilePath = string.Format("{0}{1}", ModelPath, "en-parser-chunking.bin");

            InputStream modelIn = new FileInputStream(modelFilePath);

            var model = new ParserModel(modelIn);
            Parser parser = ParserFactory.create(model);

            const string sentence = "The quick brown fox jumps over the lazy dog .";
            var parseStrings = new List<string>();
            var sb = new StringBuilder();
            List<Parse> parses = StandAloneParserTool.parseLine(sentence, parser, 5)
                .OrderBy(y => y.TagSequenceProb)
                .ToList();
            foreach (Parse parse in parses)
            {
                parse.show(sb);
                parseStrings.Add(sb.ToString());
                sb.Clear();
            }

            modelIn.close();
        }

        [Test]
        public void PostaggerCanGetTagArrayFromTestData()
        {
            string modelFilePath = string.Format("{0}{1}", ModelPath, "en-pos-maxent.bin");
            InputStream modelIn = new FileInputStream(modelFilePath);
            var model = new POSModel(modelIn);
            var tagger = new POSTaggerME(model);

            var sent = new[]
            {
                "Most", "large", "cities", "in", "the", "US", "had",
                "morning", "and", "afternoon", "newspapers", "."
            };
            string[] tags = tagger.tag(sent);
            double[] probs = tagger.probs();
            Sequence[] topSequences = tagger.topKSequences(sent);
            modelIn.close();
        }

        [Test]
        public void SentdetectCanGetSentenceArrayFromTestData()
        {
            string modelFilePath = string.Format("{0}{1}", ModelPath, "en-sent.bin");
            using (var sr = new StreamReader(string.Format("{0}{1}", InputPath, "en-sent.in.txt")))
            {
                string testTextBlock = sr.ReadToEnd();

                InputStream modelIn = new FileInputStream(modelFilePath);

                var model = new SentenceModel(modelIn);
                var sd = new SentenceDetectorME(model);
                string[] sentences = sd.sentDetect(testTextBlock);
                Assert.AreEqual(sentences.Count(), 7);
                modelIn.close();
            }
        }

        [Test]
        public void TokenizeCanGetTokensArrayFromTestData()
        {
            string modelFilePath = string.Format("{0}{1}", ModelPath, "en-token.bin");
            using (var sr = new StreamReader(string.Format("{0}{1}", InputPath, "en-sent.in.txt")))
            {
                string testTextBlock = sr.ReadToEnd();

                InputStream modelIn = new FileInputStream(modelFilePath);

                var model = new TokenizerModel(modelIn);
                var tokenizer = new TokenizerME(model);
                string[] tokens = tokenizer.tokenize(testTextBlock);
                Assert.AreEqual(tokens.Count(), 217);
                modelIn.close();
            }
        }
    }
}