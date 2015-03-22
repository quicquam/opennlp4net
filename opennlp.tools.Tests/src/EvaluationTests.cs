using System.Collections.Generic;
using NUnit.Framework;
using opennlp.tools.cmdline.sentdetect;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class EvaluationTests
    {
        private const string ModelPath = @"..\..\data\models\";
        private const string EvalPath = @"..\..\data\input\eval\";

        [Test]
        public void SentdetectEvaluationToolReturnsPrecisionRecallAndFMeasure()
        {
            var argList = new List<string>
            {
                "-model",
                string.Format("{0}{1}", ModelPath, "en-sent.bin"),
                "-data",
                string.Format("{0}{1}", EvalPath, "en-sent.eval"),
                "-encoding",
                "UTF-8"
            };

            var evaluator = new SentenceDetectorEvaluatorTool();
            evaluator.run("opennlp", argList.ToArray());
        }
    }
}
