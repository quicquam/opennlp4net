﻿using System;
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
using opennlp.model;

namespace opennlp.maxent.io
{
    /// <summary>
    /// Abstract parent class for readers of GISModels.
    /// </summary>
    public class GISModelReader : AbstractModelReader
    {
        public GISModelReader(Jfile file)
            : base(file)
        {
        }

        public GISModelReader(DataReader dataReader)
            : base(dataReader)
        {
        }

        /// <summary>
        /// Retrieve a model from disk. It assumes that models are saved in the
        /// following sequence:
        /// 
        /// <br/>
        /// GIS (model type identifier) <br/>
        /// 1. # of parameters (int) <br/>
        /// 2. the correction constant (int) <br/>
        /// 3. the correction constant parameter (double) <br/>
        /// 4. # of outcomes (int) <br/>
        /// * list of outcome names (String) <br/>
        /// 5. # of different types of outcome patterns (int) <br/>
        /// * list of (int int[]) <br/>
        /// [# of predicates for which outcome pattern is true] [outcome pattern] <br/>
        /// 6. # of predicates (int) <br/>
        /// * list of predicate names (String)
        /// 
        /// <para>
        /// If you are creating a reader for a format which won't work with this
        /// (perhaps a database or xml file), override this method and ignore the other
        /// methods provided in this abstract class.
        /// 
        /// </para>
        /// </summary>
        /// <returns> The GISModel stored in the format and location specified to this
        ///         GISModelReader (usually via its the constructor). </returns>
        public override AbstractModel constructModel()
        {
            int correctionConstant = GetCorrectionConstant();
            double correctionParam = GetCorrectionParameter();
            string[] outcomeLabels = GetOutcomes();
            int[][] outcomePatterns = GetOutcomePatterns();
            string[] predLabels = GetPredicates();
            Context[] @params = GetParameters(outcomePatterns);

            return new GISModel(@params, predLabels, outcomeLabels, correctionConstant, correctionParam);
        }

        public override void checkModelType()
        {
            string modelType = readUTF();
            if (!modelType.Equals("GIS"))
            {
                Console.WriteLine("Error: attempting to load a " + modelType + " model as a GIS model." +
                                  " You should expect problems.");
            }
        }

        protected int GetCorrectionConstant()
        {
            return readInt();
        }

        protected double GetCorrectionParameter()
        {
            return readDouble();
        }
    }
}