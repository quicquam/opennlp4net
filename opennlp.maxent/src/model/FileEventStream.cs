using System;
using System.IO;
using System.Text;
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
using j4n.IO.InputStream;
using j4n.IO.Reader;
using j4n.Object;

namespace opennlp.model
{
    using GIS = opennlp.maxent.GIS;
    using SuffixSensitiveGISModelWriter = opennlp.maxent.io.SuffixSensitiveGISModelWriter;

    /// <summary>
    /// Class for using a file of events as an event stream.  The format of the file is one event perline with
    /// each line consisting of outcome followed by contexts (space delimited).
    /// </summary>
    public class FileEventStream : AbstractEventStream, Closeable
    {
        internal BufferedReader reader;
        internal string line;

        /// <summary>
        /// Creates a new file event stream from the specified file name. </summary>
        /// <param name="fileName"> the name fo the file containing the events. </param>
        /// <exception cref="IOException"> When the specified file can not be read. </exception>

        public FileEventStream(string fileName, string encoding)
        {
            if (encoding == null)
            {
                reader = new BufferedReader(new FileReader(fileName));
            }
            else
            {
                reader = new BufferedReader(new InputStreamReader(new FileInputStream(fileName), encoding));
            }
        }

        public FileEventStream(string fileName) : this(fileName, null)
        {
        }

        /// <summary>
        /// Creates a new file event stream from the specified file. </summary>
        /// <param name="file"> the file containing the events. </param>
        /// <exception cref="IOException"> When the specified file can not be read. </exception>

        public FileEventStream(Jfile file)
        {
            reader = new BufferedReader(new InputStreamReader(new FileInputStream(file), "UTF8"));
            reader.reset();
        }

        public override bool hasNext()
        {
            try
            {
                return (null != (line = reader.readLine()));
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e);
                return (false);
            }
        }

        public override Event next()
        {
            StringTokenizer st = new StringTokenizer(line);
            string outcome = st.nextToken();
            int count = st.countTokens();
            string[] context = new string[count];
            for (int ci = 0; ci < count; ci++)
            {
                context[ci] = st.nextToken();
            }
            return (new Event(outcome, context));
        }

        public virtual void close()
        {
            reader.close();
        }

        /// <summary>
        /// Generates a string representing the specified event. </summary>
        /// <param name="event"> The event for which a string representation is needed. </param>
        /// <returns> A string representing the specified event. </returns>
        public static string toLine(Event @event)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@event.Outcome);
            string[] context = @event.Context;
            for (int ci = 0, cl = context.Length; ci < cl; ci++)
            {
                sb.Append(" ").Append(context[ci]);
            }
            sb.Append(System.Environment.NewLine); // Java getProperty("line.separator"));
            return sb.ToString();
        }

        /// <summary>
        /// Trains and writes a model based on the events in the specified event file.
        /// the name of the model created is based on the event file name. </summary>
        /// <param name="args"> eventfile [iterations cuttoff] </param>
        /// <exception cref="IOException"> when the eventfile can not be read or the model file can not be written. </exception>

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: FileEventStream eventfile [iterations cutoff]");
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
            FileEventStream es = new FileEventStream(eventFile);
            try
            {
                model = GIS.trainModel(es, iterations, cutoff);
            }
            finally
            {
                es.close();
            }
            (new SuffixSensitiveGISModelWriter(model, new Jfile(eventFile + ".bin.gz"))).persist();
        }
    }
}