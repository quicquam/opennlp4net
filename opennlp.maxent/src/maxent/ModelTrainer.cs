using System;
/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
using j4n.IO.File;
using j4n.IO.Reader;

namespace opennlp.maxent
{
    using SuffixSensitiveGISModelWriter = opennlp.maxent.io.SuffixSensitiveGISModelWriter;
    using AbstractModel = opennlp.model.AbstractModel;
    using AbstractModelWriter = opennlp.model.AbstractModelWriter;
    using EventStream = opennlp.model.EventStream;
    using OnePassDataIndexer = opennlp.model.OnePassDataIndexer;
    using OnePassRealValueDataIndexer = opennlp.model.OnePassRealValueDataIndexer;
    using PerceptronTrainer = opennlp.perceptron.PerceptronTrainer;
    using SuffixSensitivePerceptronModelWriter = opennlp.perceptron.SuffixSensitivePerceptronModelWriter;

    /// <summary>
    /// Main class which calls the GIS procedure after building the EventStream from
    /// the data.
    /// </summary>
    public class ModelTrainer
    {
        // some parameters if you want to play around with the smoothing option
        // for model training. This can improve model accuracy, though training
        // will potentially take longer and use more memory. Model size will also
        // be larger. Initial testing indicates improvements for models built on
        // small data sets and few outcomes, but performance degradation for those
        // with large data sets and lots of outcomes.
        public static bool USE_SMOOTHING = false;
        public static double SMOOTHING_OBSERVATION = 0.1;

        private static void usage()
        {
            Console.Error.WriteLine("java ModelTrainer [-real] dataFile modelFile");
            Environment.Exit(1);
        }

        /// <summary>
        /// Main method. Call as follows:
        /// <para>
        /// java ModelTrainer dataFile modelFile
        /// </para>
        /// </summary>
        public static void Main(string[] args)
        {
            int ai = 0;
            bool real = false;
            string type = "maxent";
            int maxit = 100;
            int cutoff = 1;
            double sigma = 1.0;

            if (args.Length == 0)
            {
                usage();
            }
            while (args[ai].StartsWith("-", StringComparison.Ordinal))
            {
                if (args[ai].Equals("-real"))
                {
                    real = true;
                }
                else if (args[ai].Equals("-perceptron"))
                {
                    type = "perceptron";
                }
                else if (args[ai].Equals("-maxit"))
                {
                    maxit = Convert.ToInt32(args[++ai]);
                }
                else if (args[ai].Equals("-cutoff"))
                {
                    cutoff = Convert.ToInt32(args[++ai]);
                }
                else if (args[ai].Equals("-sigma"))
                {
                    sigma = Convert.ToDouble(args[++ai]);
                }
                else
                {
                    Console.Error.WriteLine("Unknown option: " + args[ai]);
                    usage();
                }
                ai++;
            }
            string dataFileName = args[ai++];
            string modelFileName = args[ai];
            try
            {
                FileReader datafr = new FileReader(new Jfile(dataFileName));
                EventStream es;
                if (!real)
                {
                    es = new BasicEventStream(new PlainTextByLineDataStream(datafr), ",");
                }
                else
                {
                    es = new RealBasicEventStream(new PlainTextByLineDataStream(datafr));
                }

                Jfile outputFile = new Jfile(modelFileName);

                AbstractModelWriter writer;

                AbstractModel model;
                if (type.Equals("maxent"))
                {
                    GIS.SMOOTHING_OBSERVATION = SMOOTHING_OBSERVATION;

                    if (!real)
                    {
                        model = GIS.trainModel(es, maxit, cutoff, sigma);
                    }
                    else
                    {
                        model = GIS.trainModel(maxit, new OnePassRealValueDataIndexer(es, cutoff), USE_SMOOTHING);
                    }

                    writer = new SuffixSensitiveGISModelWriter(model, outputFile);
                }
                else if (type.Equals("perceptron"))
                {
                    //System.err.println("Perceptron training");
                    model = (new PerceptronTrainer()).trainModel(maxit, new OnePassDataIndexer(es, cutoff), cutoff);

                    writer = new SuffixSensitivePerceptronModelWriter(model, outputFile);
                }
                else
                {
                    throw new Exception("Unknown model type: " + type);
                }

                writer.persist();
            }
            catch (Exception e)
            {
                Console.Write("Unable to create model due to exception: ");
                Console.WriteLine(e);
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }
    }
}