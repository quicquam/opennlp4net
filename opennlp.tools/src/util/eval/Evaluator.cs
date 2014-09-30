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

        protected internal abstract T processSample(T sample);
    }
}
