using System;
using System.Collections.Generic;
using System.Linq;
using j4n.Interfaces;
using opennlp.model;
using opennlp.nonjava.helperclasses;

namespace opennlp.tools.util.model
{
    public class ModelUtil
    {
        public static TrainingParameters createTrainingParameters(int cutoff, int iterations)
        {
            throw new System.NotImplementedException();
        }

        public static bool validateOutcomes(MaxentModel model, params string[] expectedOutcomes)
        {
            bool result = true;

            if (expectedOutcomes.Length == model.NumOutcomes)
            {

                var expectedOutcomesSet = expectedOutcomes.ToList();

                for (int i = 0; i < model.NumOutcomes; i++)
                {
                    if (!expectedOutcomesSet.Contains(model.getOutcome(i)))
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        public static void writeModel(AbstractModel artifact, OutputStream @out)
        {
            throw new System.NotImplementedException();
        }
    }
}
