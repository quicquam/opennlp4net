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

            var modelFilePath = string.Format("{0}{1}", ModelPath, "en-chunker.bin");


            InputStream modelIn = new FileInputStream(modelFilePath);

            try
            {
                var model = new ChunkerModel(modelIn);
                var chunker = new ChunkerME(model);
                var tags = chunker.chunk(sent, pos);
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

        [Test]
        public void NamefinderCanGetNameArrayFromTestData()
        {
            var nameFinderModelFilePath = string.Format("{0}{1}", ModelPath, "en-ner-person.bin");
            var tokenModelPath = string.Format("{0}{1}", ModelPath, "en-token.bin");

            InputStream modelIn = new FileInputStream(nameFinderModelFilePath);

            var model = new TokenNameFinderModel(modelIn);
            var nameFinder = new NameFinderME(model);

            //1. convert sentence into tokens
            var modelInToken = new FileInputStream(tokenModelPath);
            TokenizerModel modelToken = new TokenizerModel(modelInToken);
            Tokenizer tokenizer = new TokenizerME(modelToken);
            var tokens = tokenizer.tokenize("Why is Jack London so famous?");


            var nameSpans = nameFinder.find(tokens);

            //find probabilities for names
            double[] spanProbs = nameFinder.probs(nameSpans);

            //3. print names
            for (int i = 0; i < nameSpans.Length; i++)
            {
                var s = string.Format("Span: " + nameSpans[i].ToString());
                var c =
                    string.Format("Covered text is: " + tokens[nameSpans[i].Start] + " " +
                                  tokens[nameSpans[i].End - 1]);
                var p = string.Format("Probability is: " + spanProbs[i]);
            }
            Assert.AreEqual(1, nameSpans.Count());
            Assert.AreEqual(2, nameSpans[0].Start);
            Assert.AreEqual(4, nameSpans[0].End);

            modelInToken.close();
            modelIn.close();
        }

        [Test]
        public void ParserCanGetParsesArrayFromTestData()
        {
            var modelFilePath = string.Format("{0}{1}", ModelPath, "en-parser-chunking.bin");

            InputStream modelIn = new FileInputStream(modelFilePath);

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

        [Test]
        public void PostaggerCanGetTagArrayFromTestData()
        {
            var modelFilePath = string.Format("{0}{1}", ModelPath, "en-pos-maxent.bin");
            InputStream modelIn = new FileInputStream(modelFilePath);
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
            modelIn.close();
        }

        [Test]
        public void SentdetectCanGetSentenceArrayFromTestData()
        {
            var modelFilePath = string.Format("{0}{1}", ModelPath, "en-sent2.bin");
            using (var sr = new StreamReader(string.Format("{0}{1}", InputPath, "en-sent.in.txt")))
            {
                var testTextBlock = sr.ReadToEnd();

                InputStream modelIn = new FileInputStream(modelFilePath);

                var model = new SentenceModel(modelIn);
                var sd = new SentenceDetectorME(model);
                var sentences = sd.sentDetect(testTextBlock);
                Assert.AreEqual(sentences.Count(), 7);
                modelIn.close();
            }
        }

        [Test]
        public void TokenizeCanGetTokensArrayFromTestData()
        {
            var modelFilePath = string.Format("{0}{1}", ModelPath, "en-token.bin");
            using (var sr = new StreamReader(string.Format("{0}{1}", InputPath, "en-sent.in.txt")))
            {
                var testTextBlock = sr.ReadToEnd();

                InputStream modelIn = new FileInputStream(modelFilePath);

                var model = new TokenizerModel(modelIn);
                var tokenizer = new TokenizerME(model);
                var tokens = tokenizer.tokenize(testTextBlock);
                Assert.AreEqual(tokens.Count(), 217);
                modelIn.close();
            }
        }
    }
}

