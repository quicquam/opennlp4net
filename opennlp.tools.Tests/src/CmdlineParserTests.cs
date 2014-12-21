using System.Collections.Generic;
using NUnit.Framework;
using opennlp.tools.nonjava.cmdline;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class CmdlineParserTests
    {
        // SentenceDetector en-sent.bin
        [Test]
        public void CmdlineParserIdentifiesCorrectSentenceDetectToolAndModel()
        {
            var argList = new List<string>() {"SentenceDetector", "en-sent.bin"};
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
            Assert.AreEqual(paramDictionary.Count, 2);
            Assert.AreEqual(paramDictionary["tool"], "SentenceDetector");
            Assert.AreEqual(paramDictionary["model"], "en-sent.bin");
        }

        // SentenceDetector en-sent.bin < input.txt > output.txt
        [Test]
        public void CmdlineParserIdentifiesCorrectSentenceDetectToolInputOutputAndModel()
        {
            var argList = new List<string>() { "SentenceDetector", "en-sent.bin", "<", "input.txt", ">", "output.txt"};
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
            Assert.AreEqual(paramDictionary.Count, 4);
            Assert.AreEqual(paramDictionary["tool"], "SentenceDetector");
            Assert.AreEqual(paramDictionary["model"], "en-sent.bin");
            Assert.AreEqual(paramDictionary["input"], "input.txt");
            Assert.AreEqual(paramDictionary["output"], "output.txt");
        }
        
        // SentenceDetectorTrainer[.namefinder|.conllx|.pos] [-abbDict path] [-params paramsFile] [-iterations num] [-cutoff num] -model modelFile -lang language -data sampleData [-encoding charsetName]
        [Test]
        public void CmdlineParserIdentifiesCorrectSentenceDetectToolAndAllParameters()
        {
            var argList = new List<string>()
            {
                "SentenceDetectorTrainer", 
                "-abbDict", "path", 
                "-params", "paramsFile", 
                "-iterations", "num",
                "-cutoff", "num",
                "-model", "modelFile",
                "-lang", "language",
                "-data", "sampleData",
                "-encoding", "charsetName"
            };
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
            Assert.AreEqual(paramDictionary.Count, 9);
            Assert.AreEqual(paramDictionary["tool"], "SentenceDetectorTrainer");
            Assert.AreEqual(paramDictionary["model"], "modelFile");
            Assert.AreEqual(paramDictionary["abbDict"], "path");
            Assert.AreEqual(paramDictionary["params"], "paramsFile");
            Assert.AreEqual(paramDictionary["iterations"], "num");
            Assert.AreEqual(paramDictionary["cutoff"], "num");
            Assert.AreEqual(paramDictionary["lang"], "language");
            Assert.AreEqual(paramDictionary["data"], "sampleData");
            Assert.AreEqual(paramDictionary["encoding"], "charsetName");
        }
        
        // SentenceDetectorTrainer -model en-sent.bin -lang en -data en-sent.train -encoding UTF-8
        [Test]
        public void CmdlineParserIdentifiesCorrectSentenceDetectTrainerExample()
        {
            var argList = new List<string>()
            {
                "SentenceDetectorTrainer",
                "en-sent.bin",
                "-lang", "en",
                "-data", "en-sent.train",
                "-encoding", "UTF-8"
            };
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
            Assert.AreEqual(paramDictionary.Count, 5);
            Assert.AreEqual(paramDictionary["tool"], "SentenceDetectorTrainer");
            Assert.AreEqual(paramDictionary["model"], "en-sent.bin");
            Assert.AreEqual(paramDictionary["lang"], "en");
            Assert.AreEqual(paramDictionary["data"], "en-sent.train");
            Assert.AreEqual(paramDictionary["encoding"], "UTF-8");
        }
        
        // SentenceDetectorEvaluator -model en-sent.bin -lang en -data en-sent.eval -encoding UTF-8
        [Test]
        public void CmdlineParserIdentifiesCorrectSentenceDetectorEvaluatorExample()
        {
            var argList = new List<string>()
            {
                "SentenceDetectorEvaluator",
                "-model", "en-sent.bin",
                "-lang", "en",
                "-data", "en-sent.eval",
                "-encoding", "UTF-8"
            };
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
            Assert.AreEqual(paramDictionary.Count, 5);
            Assert.AreEqual(paramDictionary["tool"], "SentenceDetectorEvaluator");
            Assert.AreEqual(paramDictionary["model"], "en-sent.bin");
            Assert.AreEqual(paramDictionary["lang"], "en");
            Assert.AreEqual(paramDictionary["data"], "en-sent.eval");
            Assert.AreEqual(paramDictionary["encoding"], "UTF-8");

        }

        // TokenizerME en-token.bin
        [Test]
        public void CmdlineParserIdentifiesCorrectTokenizerToolAndModel()
        {
            var argList = new List<string>() { "TokenizerME", "en-token.bin" };
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
            Assert.AreEqual(paramDictionary.Count, 2);
            Assert.AreEqual(paramDictionary["tool"], "TokenizerME");
            Assert.AreEqual(paramDictionary["model"], "en-token.bin");

        }

        // TokenizerME en-token.bin < article.txt > article-tokenized.txt
        [Test]
        public void CmdlineParserIdentifiesCorrectTokenizerToolInputOutputAndModel()
        {
            var argList = new List<string>() { "TokenizerME", "en-token.bin", "<", "article.txt", ">", "article-tokenized.txt" };
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
            Assert.AreEqual(paramDictionary.Count, 4);
            Assert.AreEqual(paramDictionary["tool"], "TokenizerME");
            Assert.AreEqual(paramDictionary["model"], "en-token.bin");
            Assert.AreEqual(paramDictionary["input"], "article.txt");
            Assert.AreEqual(paramDictionary["output"], "article-tokenized.txt");

        }

        // TokenizerTrainer[.namefinder|.conllx|.pos] [-abbDict path] [-alphaNumOpt isAlphaNumOpt] [-params paramsFile] [-iterations num] [-cutoff num] -model modelFile -lang language -data sampleData [-encoding charsetName]
        [Test]
        public void CmdlineParserIdentifiesCorrectTokenizerToolAndAllParameters()
        {
            var argList = new List<string>()
            {
                "TokenizerTrainer",
                "-abbDict", "path",
                "-alphaNumOpt",
                "-params", "paramsFile",
                "-iterations","num",
                "-cutoff", "num",
                "-model", "modelFile",
                "-lang", "language",
                "-data", "sampleData",
                "-encoding", "charsetName"
            };
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
            Assert.AreEqual(paramDictionary.Count, 10);
            Assert.AreEqual(paramDictionary["tool"], "TokenizerTrainer");
            Assert.AreEqual(paramDictionary["model"], "modelFile");
            Assert.AreEqual(paramDictionary["alphaNumOpt"], true);
            Assert.AreEqual(paramDictionary["abbDict"], "path");
            Assert.AreEqual(paramDictionary["params"], "paramsFile");
            Assert.AreEqual(paramDictionary["iterations"], "num");
            Assert.AreEqual(paramDictionary["cutoff"], "num");
            Assert.AreEqual(paramDictionary["lang"], "language");
            Assert.AreEqual(paramDictionary["data"], "sampleData");
            Assert.AreEqual(paramDictionary["encoding"], "charsetName");

        }

        // TokenizerTrainer -model en-token.bin -alphaNumOpt -lang en -data en-token.train -encoding UTF-8
        [Test]
        public void CmdlineParserIdentifiesCorrectTokenizerToolExample()
        {
            var argList = new List<string>()
            {
                "TokenizerTrainer",
                "-model", "en-token.bin",
                "-alphaNumOpt",
                "-lang", "en",
                "-data", "en-token.train",
                "-encoding", "UTF-8"
            };
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
            Assert.AreEqual(paramDictionary.Count, 6);
            Assert.AreEqual(paramDictionary["tool"], "TokenizerTrainer");
            Assert.AreEqual(paramDictionary["model"], "en-token.bin");
            Assert.AreEqual(paramDictionary["alphaNumOpt"], true);
            Assert.AreEqual(paramDictionary["lang"], "en");
            Assert.AreEqual(paramDictionary["data"], "en-token.train");
            Assert.AreEqual(paramDictionary["encoding"], "UTF-8");
        }
    }
}
