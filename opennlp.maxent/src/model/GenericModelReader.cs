using System;
using System.IO;
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

namespace opennlp.model
{
    using GISModelReader = opennlp.maxent.io.GISModelReader;
    using QNModelReader = opennlp.maxent.io.QNModelReader;
    using PerceptronModelReader = opennlp.perceptron.PerceptronModelReader;

    public class GenericModelReader : AbstractModelReader
    {
        private AbstractModelReader delegateModelReader;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GenericModelReader(java.io.File f) throws java.io.IOException
        public GenericModelReader(Jfile f) : base(f)
        {
        }

        public GenericModelReader(DataReader dataReader) : base(dataReader)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void checkModelType() throws java.io.IOException
        public override void checkModelType()
        {
            string modelType = readUTF();
            if (modelType.Equals("Perceptron"))
            {
                delegateModelReader = new PerceptronModelReader(this.dataReader);
            }
            else if (modelType.Equals("GIS"))
            {
                delegateModelReader = new GISModelReader(this.dataReader);
            }
            else if (modelType.Equals("QN"))
            {
                delegateModelReader = new QNModelReader(this.dataReader);
            }
            else
            {
                throw new IOException("Unknown model format: " + modelType);
            }
        }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public AbstractModel constructModel() throws java.io.IOException
        public override AbstractModel constructModel()
        {
            try
            {
                return delegateModelReader.constructModel();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
        public static void Main(string[] args)
        {
            AbstractModel m = (new GenericModelReader(new Jfile(args[0]))).Model;
            (new GenericModelWriter(m, new Jfile(args[1]))).persist();
        }
    }
}