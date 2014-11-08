using System.Collections.Generic;

namespace opennlp.tools.util
{
    public class TrainingParameters
    {
        public static string ALGORITHM_PARAM;
        public static string ITERATIONS_PARAM;
        public static string CUTOFF_PARAM;
        public IDictionary<string, string> Settings { get; set; }

        public void Put(string key, string value)
        {
            Settings.Add(new KeyValuePair<string, string>(key, value));
        }
    }
}
