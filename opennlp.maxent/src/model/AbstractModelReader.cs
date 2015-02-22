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
using j4n.IO.InputStream;
using j4n.Object;

namespace opennlp.model
{
    public abstract class AbstractModelReader
    {
        /// <summary>
        /// The number of predicates contained in the model.
        /// </summary>
        protected internal int NUM_PREDS;

        protected internal DataReader dataReader;

        protected AbstractModelReader(Jfile f)
        {
            string filename = f.Name;
            InputStream input;
            // handle the zipped/not zipped distinction
            if (filename.EndsWith(".gz", StringComparison.Ordinal))
            {
                input = new GZIPInputStream(new FileInputStream(f));
                filename = filename.Substring(0, filename.Length - 3);
            }
            else
            {
                input = new FileInputStream(f);
            }

            // handle the different formats
            if (filename.EndsWith(".bin", StringComparison.Ordinal))
            {
                dataReader = new BinaryFileDataReader(input);
            }
            else // filename ends with ".txt"
            {
                dataReader = new PlainTextFileDataReader(input);
            }
        }

        protected AbstractModelReader(DataReader dataReader)
        {
            this.dataReader = dataReader;
        }

        /// <summary>
        /// Implement as needed for the format the model is stored in.
        /// </summary>
        public virtual int readInt()
        {
            return dataReader.readInt();
        }

        /// <summary>
        /// Implement as needed for the format the model is stored in.
        /// </summary>
        public virtual double readDouble()
        {
            return dataReader.readDouble();
        }

        /// <summary>
        /// Implement as needed for the format the model is stored in.
        /// </summary>
        public virtual string readUTF()
        {
            return dataReader.readUTF();
        }

        public virtual AbstractModel Model
        {
            get
            {
                checkModelType();
                return constructModel();
            }
        }

        public abstract void checkModelType();

        public abstract AbstractModel constructModel();

        protected string[] GetOutcomes()
        {
            try
            {
                int numOutcomes = readInt();
                string[] outcomeLabels = new string[numOutcomes];
                for (int i = 0; i < numOutcomes; i++)
                {
                    outcomeLabels[i] = readUTF();
                }
                return outcomeLabels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected int[][] GetOutcomePatterns()
        {
            try
            {
                int numOCTypes = readInt();
                int[][] outcomePatterns = new int[numOCTypes][];
                for (int i = 0; i < numOCTypes; i++)
                {
                    StringTokenizer tok = new StringTokenizer(readUTF(), " ");
                    int[] infoInts = new int[tok.countTokens()];
                    for (int j = 0; tok.hasMoreTokens(); j++)
                    {
                        infoInts[j] = Convert.ToInt32(tok.nextToken());
                    }
                    outcomePatterns[i] = infoInts;
                }
                return outcomePatterns;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected string[] GetPredicates()
        {
            try
            {
                NUM_PREDS = readInt();
                string[] predLabels = new string[NUM_PREDS];
                for (int i = 0; i < NUM_PREDS; i++)
                {
                    predLabels[i] = readUTF();
                }
                return predLabels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Reads the parameters from a file and populates an array of context objects. </summary>
        /// <param name="outcomePatterns"> The outcomes patterns for the model.  The first index refers to which 
        /// outcome pattern (a set of outcomes that occurs with a context) is being specified.  The
        /// second index specifies the number of contexts which use this pattern at index 0, and the
        /// index of each outcomes which make up this pattern in indicies 1-n. </param>
        /// <returns> An array of context objects. </returns>
        /// <exception cref="java.io.IOException"> when the model file does not match the outcome patterns or can not be read. </exception>
        protected Context[] GetParameters(int[][] outcomePatterns)
        {
            Context[] parameters = new Context[NUM_PREDS];
            int pid = 0;
            for (int i = 0; i < outcomePatterns.Length; i++)
            {
                //construct outcome pattern
                int[] outcomePattern = new int[outcomePatterns[i].Length - 1];
                for (int k = 1; k < outcomePatterns[i].Length; k++)
                {
                    outcomePattern[k - 1] = outcomePatterns[i][k];
                }
                //System.err.println("outcomePattern "+i+" of "+outcomePatterns.length+" with "+outcomePatterns[i].length+" outcomes ");
                //populate parameters for each context which uses this outcome pattern. 
                for (int j = 0; j < outcomePatterns[i][0]; j++)
                {
                    double[] contextParameters = new double[outcomePatterns[i].Length - 1];
                    for (int k = 1; k < outcomePatterns[i].Length; k++)
                    {
                        contextParameters[k - 1] = readDouble();
                    }
                    parameters[pid] = new Context(outcomePattern, contextParameters);
                    pid++;
                }
            }
            return parameters;
        }
    }
}