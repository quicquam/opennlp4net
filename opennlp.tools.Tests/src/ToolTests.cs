using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using opennlp.tools.cmdline.chunker;
using opennlp.tools.cmdline.namefind;
using opennlp.tools.cmdline.parser;
using opennlp.tools.cmdline.postag;
using opennlp.tools.cmdline.sentdetect;
using opennlp.tools.cmdline.tokenizer;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class ToolTests
    {
        private const string ModelPath = @"..\..\data\models\";
        private const string InputPath = @"..\..\data\input\";
        private const string OutputPath = @"..\..\data\output\";
        
        private const string ChunkerMeFileBase = "en-chunker";
        private const string ParserFileBase = "en-parser-chunking";
        private const string PosTaggerFileBase = "en-pos-maxent";
        private const string SentenceDectectorFileBase = "en-sent";
        private const string TaggerModelReplacerFileBase = "en-parser-chunking";
        private const string TaggerModelReplacerSecondFile = "en-pos-maxent";

        private const string TokenizerMeFileBase = "en-token";
        private const string TokenNameFinderFileBase = "en-ner-person";

        private List<string> GenerateModelFilenames(string toolFileBase)
        {
            var inputFileName = string.Format("{0}{1}.in.txt", InputPath, toolFileBase);
            var outputFileName = string.Format("{0}{1}.out.txt", OutputPath, toolFileBase);
            var modelFileName = string.Format("{0}{1}.bin", ModelPath, toolFileBase);

            return new List<string> { modelFileName, inputFileName, outputFileName };
        }

        [Test]
        public void ChunkerMEToolLoadsInputRunsAndCreatesOutput()
        {
            var chunkerMeTool = new ChunkerMETool();
            var fileList = GenerateModelFilenames(ChunkerMeFileBase);
            chunkerMeTool.run(fileList.ToArray());
            var output = File.ReadAllText(fileList[2], Encoding.Unicode);
        }

        [Test]
        public void ParserToolLoadsInputRunsAndCreatesOutput()
        {
            var parserTool = new ParserTool();
            var fileList = GenerateModelFilenames(ParserFileBase);
            parserTool.run(fileList.ToArray());
            var output = File.ReadAllText(fileList[2], Encoding.Unicode);            
        }

        [Test]
        public void POSTaggerToolLoadsInputRunsAndCreatesOutput()
        {
            var posTaggerTool = new POSTaggerTool();
            var fileList = GenerateModelFilenames(PosTaggerFileBase);
            posTaggerTool.run(fileList.ToArray());
            var output = File.ReadAllText(fileList[2], Encoding.Unicode);
        }

        [Test]
        public void SentenceDetectToolLoadsInputRunsAndCreatesOutput()
        {
            var sentenceDetectTool = new SentenceDetectorTool();
            var fileList = GenerateModelFilenames(SentenceDectectorFileBase);
            sentenceDetectTool.run(fileList.ToArray());
            var lines = File.ReadAllLines(fileList[2], Encoding.Unicode);
            foreach (var line in lines)
            {
                ;
            }
        }

        [Ignore]
        [Test]
        public void TaggerModelReplacerToolLoadsInputRunsAndCreatesOutput()
        {
            var taggerModelReplacerTool = new TaggerModelReplacerTool();
            var fileList = new List<string>
            {
                string.Format("{0}{1}.bin", ModelPath, TaggerModelReplacerFileBase),
                string.Format("{0}{1}.bin", ModelPath, TaggerModelReplacerSecondFile)
            };
            taggerModelReplacerTool.run(fileList.ToArray());
            var output = File.ReadAllText(fileList[2], Encoding.Unicode);            
        }

        [Test]
        public void TokenizerMEToolLoadsInputRunsAndCreatesOutput()
        {
            var tokenizerMeTool = new TokenizerMETool();
            var fileList = GenerateModelFilenames(TokenizerMeFileBase);
            tokenizerMeTool.run(fileList.ToArray());
            var output = File.ReadAllText(fileList[2], Encoding.Unicode);
        }

        [Test]
        public void TokenNameFinderToolLoadsInputRunsAndCreatesOutput()
        {
            var tokenNameFinderTool = new TokenNameFinderTool();
            var fileList = GenerateModelFilenames(TokenNameFinderFileBase);
            tokenNameFinderTool.run(fileList.ToArray());
            var output = File.ReadAllText(fileList[2], Encoding.Unicode);
        }
    }
}
