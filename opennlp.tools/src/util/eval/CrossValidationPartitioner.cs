namespace opennlp.tools.util.eval
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
            public ObjectStream<T> TestSampleStream { get; set; }
            public override T read()
            {
                throw new System.NotImplementedException();
            }
        }

        public TrainingSampleStream next()
        {
            throw new System.NotImplementedException();
        }
    }
}