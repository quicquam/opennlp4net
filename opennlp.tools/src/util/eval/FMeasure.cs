﻿namespace opennlp.tools.util.eval
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

        public string getFMeasure()
        {
            throw new System.NotImplementedException();
        }

        public string RecallScore { get; set; }
        public string PrecisionScore { get; set; }
    }
}