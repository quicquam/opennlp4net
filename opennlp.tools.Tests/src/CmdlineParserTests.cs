﻿using System.Collections.Generic;
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
        }

        // SentenceDetector en-sent.bin < input.txt > output.txt
        [Test]
        public void CmdlineParserIdentifiesCorrectSentenceDetectToolInputOutputAndModel()
        {
            var argList = new List<string>() { "SentenceDetector", "en-sent.bin", "<", "input.txt", ">", "output.txt"};
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
        }
        
        // SentenceDetectorTrainer[.namefinder|.conllx|.pos] [-abbDict path] [-params paramsFile] [-iterations num] [-cutoff num] -model modelFile -lang language -data sampleData [-encoding charsetName]
        [Test]
        public void CmdlineParserIdentifiesCorrectSentenceDetectToolAndAllParameters()
        {
            var argList = new List<string>()
            {
                "SentenceDetectorTrainer", 
                "-abbDict", " path", 
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
        }
        
        // SentenceDetectorTrainer -model en-sent.bin -lang en -data en-sent.train -encoding UTF-8
        [Test]
        public void CmdlineParserIdentifiesCorrectSentenceDetectTrainerExample()
        {
            var argList = new List<string>() { "SentenceDetectorTrainer", "en-sent.bin", "-lang", "en", "-data", "en-sent.train", "-encoding", "UTF-8" };
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
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
        }

        // TokenizerME en-token.bin
        [Test]
        public void CmdlineParserIdentifiesCorrectTokenizerToolAndModel()
        {
            var argList = new List<string>() { "TokenizerME", "en-token.bin" };
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
        }

        // TokenizerME en-token.bin < article.txt > article-tokenized.txt
        [Test]
        public void CmdlineParserIdentifiesCorrectTokenizerToolInputOutputAndModel()
        {
            var argList = new List<string>() { "TokenizerME", "en-token.bin", "<", "article.txt", ">", "article-tokenized.txt" };
            var cmdLineParser = new CmdlineParser();
            var paramDictionary = cmdLineParser.Parse(argList.ToArray());
        }

        // TokenizerTrainer[.namefinder|.conllx|.pos] [-abbDict path] [-alphaNumOpt isAlphaNumOpt] [-params paramsFile] [-iterations num] [-cutoff num] -model modelFile -lang language -data sampleData [-encoding charsetName]
        [Test]
        public void CmdlineParserIdentifiesCorrectTokenizerToolAndAllParameters()
        {
            var argList = new List<string>()
            {
                "TokenizerTrainer",
                "-abbDict", " path",
                "-alphaNumOpt", "isAlphaNumOpt",
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
        }
    }
}
