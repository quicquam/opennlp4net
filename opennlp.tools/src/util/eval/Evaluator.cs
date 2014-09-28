namespace opennlp.tools.util.eval
{
    public abstract class Evaluator<T>
    {
        protected internal abstract T processSample(T sample);
    }
}
