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

namespace opennlp.maxent.io
{
    using QNModel = opennlp.maxent.quasinewton.QNModel;
    using AbstractModel = opennlp.model.AbstractModel;
    using AbstractModelReader = opennlp.model.AbstractModelReader;
    using Context = opennlp.model.Context;
    using DataReader = opennlp.model.DataReader;

    public class QNModelReader : AbstractModelReader
    {
        public QNModelReader(DataReader dataReader) : base(dataReader)
        {
        }

        public QNModelReader(Jfile file) : base(file)
        {
        }

        public override void checkModelType()
        {
            string modelType = readUTF();
            if (!modelType.Equals("QN"))
            {
                Console.WriteLine("Error: attempting to load a " + modelType + " model as a MAXENT_QN model." +
                                  " You should expect problems.");
            }
        }

        public override AbstractModel constructModel()
        {
            string[] predNames = GetPredicates();
            string[] outcomeNames = GetOutcomes();
            Context[] cParameters = Parameters;
            double[] parameters = DoubleArrayParams;
            return new QNModel(predNames, outcomeNames, cParameters, parameters);
        }

        private double[] DoubleArrayParams
        {
            get
            {
                int numDouble = readInt();
                double[] doubleArray = new double[numDouble];
                for (int i = 0; i < numDouble; i++)
                {
                    doubleArray[i] = readDouble();
                }
                return doubleArray;
            }
        }

        private int[] IntArrayParams
        {
            get
            {
                int numInt = readInt();
                int[] intArray = new int[numInt];
                for (int i = 0; i < numInt; i++)
                {
                    intArray[i] = readInt();
                }
                return intArray;
            }
        }

        protected internal virtual Context[] Parameters
        {
            get
            {
                int numContext = readInt();
                Context[] cParameters = new Context[numContext];

                for (int i = 0; i < numContext; i++)
                {
                    int[] outcomePattern = IntArrayParams;
                    double[] parameters = DoubleArrayParams;
                    cParameters[i] = new Context(outcomePattern, parameters);
                }
                return cParameters;
            }
        }
    }
}