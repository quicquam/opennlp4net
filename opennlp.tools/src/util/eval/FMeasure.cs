namespace opennlp.tools.util.eval
{
    public class FMeasure
    {
        public void updateScores(Span[] references, Span[] predictions)
        {
            throw new System.NotImplementedException();
        }

        public void mergeInto(FMeasure fMeasure)
        {
            throw new System.NotImplementedException();
        }

        public double getFMeasure()
        {
            throw new System.NotImplementedException();
        }

        public double RecallScore { get; set; }
        public double PrecisionScore { get; set; }
    }
}