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
using j4n.Interfaces;
using j4n.IO.File;
using j4n.IO.OutputStream;
using j4n.IO.Writer;

namespace opennlp.maxent.io
{
    using AbstractModel = opennlp.model.AbstractModel;

    /// <summary>
    /// A writer for GIS models which inspects the filename and invokes the
    /// appropriate GISModelWriter depending on the filename's suffixes.
    /// 
    /// <para>The following assumption are made about suffixes:
    ///    <li>.gz  --> the file is gzipped (must be the last suffix)
    ///    <li>.txt --> the file is plain text
    ///    <li>.bin --> the file is binary
    /// </para>
    /// </summary>
    public class SuffixSensitiveGISModelWriter : GISModelWriter
    {
        private readonly GISModelWriter suffixAppropriateWriter;

        /// <summary>
        /// Constructor which takes a GISModel and a File and invokes the
        /// GISModelWriter appropriate for the suffix.
        /// </summary>
        /// <param name="model"> The GISModel which is to be persisted. </param>
        /// <param name="f"> The File in which the model is to be stored. </param>

        public SuffixSensitiveGISModelWriter(AbstractModel model, Jfile f) : base(model)
        {
            OutputStream output;
            string filename = f.Name;

            // handle the zipped/not zipped distinction
            if (filename.EndsWith(".gz", StringComparison.Ordinal))
            {
                output = new GZIPOutputStream(new FileOutputStream(f));
                filename = filename.Substring(0, filename.Length - 3);
            }
            else
            {
                output = new DataOutputStream(new FileOutputStream(f));
            }

            // handle the different formats
            if (filename.EndsWith(".bin", StringComparison.Ordinal))
            {
                suffixAppropriateWriter = new BinaryGISModelWriter(model, new DataOutputStream(output));
            }
            else // default is ".txt"
            {
                suffixAppropriateWriter = new PlainTextGISModelWriter(model,
                    new BufferedWriter(new OutputStreamWriter(output)));
            }
        }


        public override void writeUTF(string s)
        {
            suffixAppropriateWriter.writeUTF(s);
        }


        public override void writeInt(int i)
        {
            suffixAppropriateWriter.writeInt(i);
        }


        public override void writeDouble(double d)
        {
            suffixAppropriateWriter.writeDouble(d);
        }

        public override void close()
        {
            suffixAppropriateWriter.close();
        }
    }
}