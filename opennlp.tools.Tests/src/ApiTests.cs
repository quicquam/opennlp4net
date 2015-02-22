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
    public class ApiTests : TestsBase
    {
        private const string ModelPath = @"..\..\data\models\";
        private const string InputPath = @"..\..\data\input\";
        private const string VerifyPath = @"..\..\data\refoutput\";

        [Test]
        public void ChunkerCanGetTokensArrayFromTestData()
        {
            using (var sr = new StreamReader(string.Format("{0}{1}", InputPath, "en-chunker.in.txt")))
            {
                string testTextBlock = sr.ReadToEnd();
                POSSample posSample = POSSample.parse(testTextBlock);

                string modelFilePath = string.Format("{0}{1}", ModelPath, "en-chunker.bin");

                InputStream modelIn = new FileInputStream(modelFilePath);

                var model = new ChunkerModel(modelIn);
                var chunker = new ChunkerME(model);

                var tags = chunker.chunk(posSample.Sentence, posSample.Tags);
                modelIn.close();
               
                var verificationArray = GetVerificationStrings(string.Format("{0}{1}", VerifyPath, "en-chunker.ref.out"));

                Assert.AreEqual(45, tags.Count());
                Assert.AreEqual(verificationArray, tags);

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

            //3. print names
            var nameList = new List<string>();
            foreach (Span t in nameSpans)
            {
                string name = "";
                for (int j = t.Start; j < t.End; j++)
                {
                    name += tokens[j] + " ";
                }

                name = name.TrimEnd(new[] { ' ' });
                nameList.Add(name);
            }

            modelInToken.close();
            modelIn.close();

            var verificationArray = GetVerificationStrings(string.Format("{0}{1}", VerifyPath, "en-ner-location.ref.out"));

            Assert.AreEqual(5, nameSpans.Count());
            Assert.AreEqual(verificationArray, nameList.ToArray());
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
            string[] tokens = tokenizer.tokenize("Bleak House was written by Charles Dickens, while he lived at Tavistock House.");

            Span[] nameSpans = nameFinder.find(tokens);

            var nameList = new List<string>();
            foreach (Span t in nameSpans)
            {
                string name = "";
                for (int j = t.Start; j < t.End; j++)
                {
                    name += tokens[j] + " ";
                }

                name = name.TrimEnd(new[] { ' ' });
                nameList.Add(name);
            }

            modelInToken.close();
            modelIn.close();

            var verificationArray = GetVerificationStrings(string.Format("{0}{1}", VerifyPath, "en-ner-person.ref.out"));

            Assert.AreEqual(4, nameSpans.Count());
            Assert.AreEqual(verificationArray, nameList.ToArray());

        }

        [Test]
        public void ParserCanGetParsesArrayFromTestData()
        {
            string modelFilePath = string.Format("{0}{1}", ModelPath, "en-parser-chunking.bin");

            InputStream modelIn = new FileInputStream(modelFilePath);

            var model = new ParserModel(modelIn);
            Parser parser = ParserFactory.create(model);

            using (var sr = new StreamReader(string.Format("{0}{1}", InputPath, "en-parser-chunking.in.txt")))
            {
                var parse = StandAloneParserTool.parseLine(sr.ReadToEnd(), parser, 1);
                var parseAsString = new StringBuilder();

                parse[0].show(parseAsString);
 
                modelIn.close();

                var verificationString = GetVerificationString(string.Format("{0}{1}", VerifyPath, "en-parser-chunking.ref.out"));
                Assert.AreEqual(parseAsString.ToString(), verificationString);
            }
        }

        [Test]
        public void PostaggerCanGetTagArrayFromTestData()
        {
            string modelFilePath = string.Format("{0}{1}", ModelPath, "en-pos-maxent.bin");
            InputStream modelIn = new FileInputStream(modelFilePath);

            string tokenModelFilePath = string.Format("{0}{1}", ModelPath, "en-token.bin");
            InputStream tokenModelIn = new FileInputStream(tokenModelFilePath);
            var tokenModel = new TokenizerModel(tokenModelIn);

            var model = new POSModel(modelIn);
            var tagger = new POSTaggerME(model);

            using (var sr = new StreamReader(string.Format("{0}{1}", InputPath, "en-pos-maxent.in.txt")))
            {
                string testTextBlock = sr.ReadToEnd();
                var tokenizer = new TokenizerME(tokenModel);
                string[] tokens = tokenizer.tokenize(testTextBlock);

                string[] tags = tagger.tag(tokens);
                double[] probs = tagger.probs();
                Sequence[] topSequences = tagger.topKSequences(tokens);

                modelIn.close();

                var verificationArray = GetVerificationStrings(string.Format("{0}{1}", VerifyPath, "en-pos-maxent.ref.out"));
                Assert.AreEqual(topSequences[0].Outcomes.Count, verificationArray.Count());
                Assert.AreEqual(topSequences[0].Outcomes, verificationArray);
            }
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
                modelIn.close();

                var verificationArray = GetVerificationStrings(string.Format("{0}{1}", VerifyPath, "en-sent.ref.out"));

                Assert.AreEqual(sentences.Count(), verificationArray.Count());
                Assert.AreEqual(verificationArray, sentences);

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

                modelIn.close();

                var verificationArray = GetVerificationStrings(string.Format("{0}{1}", VerifyPath, "en-token.ref.out"));

                Assert.AreEqual(tokens.Count(), verificationArray.Count());
                Assert.AreEqual(verificationArray, tokens);
            }
        }
    }
}