using j4n.Serialization;
using opennlp.tools.tokenize;

namespace opennlp.tools.util
{
    public class CrossValidationPartitioner<T>
    {
        public CrossValidationPartitioner(ObjectStream<T> samples, int nFolds)
        {
            throw new System.NotImplementedException();
        }

        public bool hasNext()
        {
            throw new System.NotImplementedException();
        }

        public class TrainingSampleStream : ObjectStream<T>
        {
            public object TestSampleStream { get; set; }
        }

        public TrainingSampleStream next()
        {
            throw new System.NotImplementedException();
        }
    }
}
