using System;
using System.IO;
using System.Linq;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using NUnit.Framework;
using opennlp.tools.sentdetect;
using opennlp.tools.util;

namespace opennlp.tools.Tests
{
    [TestFixture]
    public class TrainingApiTests
    {
        private const string ModelOutputPath = @"..\..\data\models\out\";
        private const string InputPath = @"..\..\data\input\train\";


        [Test]
        public void SentdetectCanGetSentenceArrayFromTestData()
        {
            var modelFilePath = string.Format("{0}{1}", ModelOutputPath, "en-sent.bin");
            var trainingFilePath = string.Format("{0}{1}", InputPath, "en-sent.train");
            Charset charset = Charset.forName("UTF-8");
            ObjectStream<String> lineStream =
              new PlainTextByLineStream(new FileInputStream(trainingFilePath), charset);
            ObjectStream<SentenceSample> sampleStream = new SentenceSampleStream(lineStream);

            SentenceModel model;

            try
            {
                model = SentenceDetectorME.train("en", sampleStream, true, null, TrainingParameters.defaultParams());
            }
            finally
            {
                sampleStream.close();
            }

            OutputStream modelOut = null;
            try
            {
                modelOut = new FileOutputStream(modelFilePath);
                model.serialize(modelOut as FileOutputStream);
            }
            finally
            {
                if (modelOut != null)
                    modelOut.close();
            }
        }
    }
}
