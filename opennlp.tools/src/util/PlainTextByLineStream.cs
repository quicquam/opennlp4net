/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.Reader;

namespace opennlp.tools.util
{
    /// <summary>
    /// Reads a plain text file and return each line as a <code>String</code> object.
    /// </summary>
    public class PlainTextByLineStream : ObjectStream<string>
    {
        private readonly FileChannel channel;
        private readonly string encoding;

        private BufferedReader input;

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="in"> </param>
        public PlainTextByLineStream(Reader @in)
        {
            this.input = new BufferedReader(@in);
            this.channel = null;
            this.encoding = null;
        }

        public PlainTextByLineStream(InputStream @in, string charsetName)
            : this(new InputStreamReader(@in, charsetName))
        {
        }

        public PlainTextByLineStream(InputStream @in, Charset charset) : this(new InputStreamReader(@in, charset))
        {
        }

        public PlainTextByLineStream(FileChannel channel, string charsetName)
        {
            this.encoding = charsetName;
            this.channel = channel;

            // TODO: Why isn't reset called here ?
            input = new BufferedReader(Channels.newReader(channel, encoding));
        }

        public PlainTextByLineStream(FileChannel channel, Charset encoding) : this(channel, encoding.name())
        {
        }

        public PlainTextByLineStream(InputStreamReader @in)
        {
            input = new BufferedReader(@in);
        }

        public override string read()
        {
            return input.readLine();
        }

        public override void reset()
        {
            if (channel == null)
            {
                input.reset();
            }
            else
            {
                channel.position(0);
                input = new BufferedReader(Channels.newReader(channel, encoding));
            }
        }

        public override void close()
        {
            if (channel == null)
            {
                input.close();
            }
            else
            {
                channel.close();
            }
        }
    }
}