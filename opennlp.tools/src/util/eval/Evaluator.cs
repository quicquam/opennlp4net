using System;
using opennlp.tools.namefind;
using opennlp.tools.sentdetect;
using opennlp.tools.tokenize;

namespace opennlp.tools.util.eval
{
    public abstract class Evaluator<T>
    {
        protected Evaluator(EvaluationMonitor<T>[] listeners)
        {
            throw new System.NotImplementedException();
        }


        public void evaluate(ObjectStream<T> iterator)
        {
            throw new NotImplementedException();
        }


        protected internal abstract T processSample(T sample);
    }
}