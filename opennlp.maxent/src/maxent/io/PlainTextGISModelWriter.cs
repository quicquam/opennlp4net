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
using j4n.IO.OutputStream;
using j4n.IO.Writer;

namespace opennlp.maxent.io
{
    using AbstractModel = opennlp.model.AbstractModel;

    /// <summary>
    /// Model writer that saves models in plain text format.
    /// </summary>
    public class PlainTextGISModelWriter : GISModelWriter
    {
        internal BufferedWriter output;

        /// <summary>
        /// Constructor which takes a GISModel and a File and prepares itself to
        /// write the model to that file. Detects whether the file is gzipped or not
        /// based on whether the suffix contains ".gz".
        /// </summary>
        /// <param name="model"> The GISModel which is to be persisted. </param>
        /// <param name="f"> The File in which the model is to be persisted. </param>

        public PlainTextGISModelWriter(AbstractModel model, Jfile f) : base(model)
        {
            if (f.Name.EndsWith(".gz", StringComparison.Ordinal))
            {
                output = new BufferedWriter(new OutputStreamWriter(new GZIPOutputStream(new FileOutputStream(f))));
            }
            else
            {
                output = new BufferedWriter(new FileWriter(f));
            }
        }

        /// <summary>
        /// Constructor which takes a GISModel and a BufferedWriter and prepares
        /// itself to write the model to that writer.
        /// </summary>
        /// <param name="model"> The GISModel which is to be persisted. </param>
        /// <param name="bw"> The BufferedWriter which will be used to persist the model. </param>
        public PlainTextGISModelWriter(AbstractModel model, BufferedWriter bw) : base(model)
        {
            output = bw;
        }

        public override void writeUTF(string s)
        {
            output.write(s);
            output.newLine();
        }

        public override void writeInt(int i)
        {
            output.write(Convert.ToString(i));
            output.newLine();
        }

        public override void writeDouble(double d)
        {
            output.write(Convert.ToString(d));
            output.newLine();
        }

        public override void close()
        {
            output.flush();
            output.close();
        }
    }
}