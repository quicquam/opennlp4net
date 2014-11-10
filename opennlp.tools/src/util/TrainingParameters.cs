/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.Utils;

namespace opennlp.tools.util
{

    public class TrainingParameters
    {

        public static string ALGORITHM_PARAM = "Algorithm";

        public static string ITERATIONS_PARAM = "Iterations";
        public static string CUTOFF_PARAM = "Cutoff";

        private IDictionary<string, string> parameters = new Dictionary<string, string>();

        public TrainingParameters()
        {
        }

        public TrainingParameters(InputStream @in)
        {

            Properties properties = new Properties();
            properties.load(@in);
            /* TODO load properties
                for (Map.Entry<Object, Object> entry : properties.entrySet()) {
                  parameters.put((String) entry.getKey(), (String) entry.getValue());
                }
            */
        }

        /**
         * Retrieves the training algorithm name for a given name space.
         * 
         * @return the name or null if not set.
         */
        public String algorithm(String name)
        {
            string outVal;
            return parameters.TryGetValue(name + "." + ALGORITHM_PARAM, out outVal) ? outVal : "";
        }

        /**
         * Retrieves the training algorithm name.
         * 
         * @return the name or null if not set.
         */
        public String algorithm()
        {
            string outVal;
            return parameters.TryGetValue(ALGORITHM_PARAM, out outVal) ? outVal : "";
        }

        /**
         * Retrieves a map with the training parameters which have the passed name space.
         * 
         * @param namespace
         * 
         * @return a parameter map which can be passed to the train and validate methods.
         */
        public Dictionary<String, String> getSettings(string nameSpace)
        {

            var trainingParams = new Dictionary<String, String>();
            foreach (KeyValuePair<string, string> entry in parameters)
            {
                var key = entry.Key;

                if (nameSpace != null)
                {
                    var prefix = nameSpace + ".";

                    if (key.StartsWith(prefix))
                    {
                        trainingParams.Add(key.Substring(prefix.Length), entry.Value);
                    }
                }
                else
                {
                    if (!key.Contains("."))
                    {
                        trainingParams.Add(key, entry.Value);
                    }
                }

            }

            return trainingParams;
        }

        /** 
         * Retrieves all parameters without a name space.
         * 
         * @return the settings map
         */
        public Dictionary<String, String> getSettings()
        {
            return getSettings(null);
        }

        // reduces the params to contain only the params in the name space
        public TrainingParameters getParameters(String nameSpace)
        {

            var parameters = new TrainingParameters();

            foreach (var entry in getSettings(nameSpace))
            {
                parameters.put(entry.Key, entry.Value);
            }

            return parameters;
        }

        public void put(String nameSpace, String key, String value)
        {

            if (nameSpace == null)
            {
                parameters.Add(key, value);
            }
            else
            {
                parameters.Add(nameSpace + "." + key, value);
            }
        }

        public void put(String key, String value)
        {
            put(null, key, value);
        }

        public void serialize(OutputStream @out)
        {
            Properties properties = new Properties();

            foreach (var entry in parameters)
            {
                properties.Add(entry.Key, entry.Value);
            }

            properties.store(@out, null);
        }

        public static TrainingParameters defaultParams()
        {
            TrainingParameters mlParams = new TrainingParameters();
            mlParams.put(ALGORITHM_PARAM, "MAXENT");
            mlParams.put(ITERATIONS_PARAM, "100");
            mlParams.put(CUTOFF_PARAM, "5");

            return mlParams;
        }
    }
}
