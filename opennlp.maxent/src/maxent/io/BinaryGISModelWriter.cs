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
using j4n.IO.OutputStream;

namespace opennlp.maxent.io
{
    using AbstractModel = opennlp.model.AbstractModel;

    /// <summary>
    /// Model writer that saves models in binary format.
    /// </summary>
    public class BinaryGISModelWriter : GISModelWriter
    {
        internal DataOutputStream output;

        /// <summary>
        /// Constructor which takes a GISModel and a File and prepares itself to write
        /// the model to that file. Detects whether the file is gzipped or not based on
        /// whether the suffix contains ".gz".
        /// </summary>
        /// <param name="model">
        ///          The GISModel which is to be persisted. </param>
        /// <param name="f">
        ///          The File in which the model is to be persisted. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public BinaryGISModelWriter(opennlp.model.AbstractModel model, java.io.File f) throws java.io.IOException
        public BinaryGISModelWriter(AbstractModel model, Jfile f) : base(model)
        {
            if (f.Name.EndsWith(".gz", StringComparison.Ordinal))
            {
                output = new DataOutputStream(new GZIPOutputStream(new FileOutputStream(f)));
            }
            else
            {
                output = new DataOutputStream(new FileOutputStream(f));
            }
        }

        /// <summary>
        /// Constructor which takes a GISModel and a DataOutputStream and prepares
        /// itself to write the model to that stream.
        /// </summary>
        /// <param name="model">
        ///          The GISModel which is to be persisted. </param>
        /// <param name="dos">
        ///          The stream which will be used to persist the model. </param>
        public BinaryGISModelWriter(AbstractModel model, DataOutputStream dos) : base(model)
        {
            output = dos;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeUTF(String s) throws java.io.IOException
        public override void writeUTF(string s)
        {
            output.writeUTF(s);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeInt(int i) throws java.io.IOException
        public override void writeInt(int i)
        {
            output.writeInt(i);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeDouble(double d) throws java.io.IOException
        public override void writeDouble(double d)
        {
            output.writeDouble(d);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
        public override void close()
        {
            output.flush();
            output.close();
        }
    }
}