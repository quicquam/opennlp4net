using System;
using System.IO;
using j4n.Exceptions;
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
using opennlp.nonjava.helperclasses;

namespace opennlp.model
{
    using GIS = opennlp.maxent.GIS;
    using SuffixSensitiveGISModelWriter = opennlp.maxent.io.SuffixSensitiveGISModelWriter;

    public class RealValueFileEventStream : FileEventStream
    {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RealValueFileEventStream(String fileName) throws java.io.IOException
        public RealValueFileEventStream(string fileName) : base(fileName)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RealValueFileEventStream(String fileName, String encoding) throws java.io.IOException
        public RealValueFileEventStream(string fileName, string encoding) : base(fileName, encoding)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RealValueFileEventStream(java.io.File file) throws java.io.IOException
        public RealValueFileEventStream(Jfile file) : base(file)
        {
        }

        /// <summary>
        /// Parses the specified contexts and re-populates context array with features
        /// and returns the values for these features. If all values are unspecified,
        /// then null is returned.
        /// </summary>
        /// <param name="contexts"> The contexts with real values specified. </param>
        /// <returns> The value for each context or null if all values are unspecified. </returns>
        public static float[] parseContexts(string[] contexts)
        {
            bool hasRealValue = false;
            float[] values = new float[contexts.Length];
            for (int ci = 0; ci < contexts.Length; ci++)
            {
                int ei = contexts[ci].LastIndexOf("=", StringComparison.Ordinal);
                if (ei > 0 && ei + 1 < contexts[ci].Length)
                {
                    bool gotReal = true;
                    try
                    {
                        values[ci] = Convert.ToSingle(contexts[ci].Substring(ei + 1));
                    }
                    catch (NumberFormatException)
                    {
                        gotReal = false;
                        Console.Error.WriteLine("Unable to determine value in context:" + contexts[ci]);
                        values[ci] = 1;
                    }
                    if (gotReal)
                    {
                        if (values[ci] < 0)
                        {
                            throw new Exception("Negative values are not allowed: " + contexts[ci]);
                        }
                        contexts[ci] = contexts[ci].Substring(0, ei);
                        hasRealValue = true;
                    }
                }
                else
                {
                    values[ci] = 1;
                }
            }
            if (!hasRealValue)
            {
                values = null;
            }
            return values;
        }

        public override Event next()
        {
            int si = line.IndexOf(' ');
            string outcome = line.Substring(0, si);
            string[] contexts = line.Substring(si + 1).Split(" ", true);
            float[] values = parseContexts(contexts);
            return (new Event(outcome, contexts, values));
        }

        /// <summary>
        /// Trains and writes a model based on the events in the specified event file.
        /// the name of the model created is based on the event file name.
        /// </summary>
        /// <param name="args"> eventfile [iterations cuttoff] </param>
        /// <exception cref="IOException"> when the eventfile can not be read or the model file can not be written. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: RealValueFileEventStream eventfile [iterations cutoff]");
                Environment.Exit(1);
            }
            int ai = 0;
            string eventFile = args[ai++];
            int iterations = 100;
            int cutoff = 5;
            if (ai < args.Length)
            {
                iterations = Convert.ToInt32(args[ai++]);
                cutoff = Convert.ToInt32(args[ai++]);
            }
            AbstractModel model;
            RealValueFileEventStream es = new RealValueFileEventStream(eventFile);
            try
            {
                model = GIS.trainModel(iterations, new OnePassRealValueDataIndexer(es, cutoff));
            }
            finally
            {
                es.close();
            }
            (new SuffixSensitiveGISModelWriter(model, new Jfile(eventFile + ".bin.gz"))).persist();
        }
    }
}