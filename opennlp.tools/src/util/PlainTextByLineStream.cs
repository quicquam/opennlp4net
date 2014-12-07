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
using j4n.Serialization;

namespace opennlp.tools.util
{
    /// <summary>
    /// Reads a plain text file and return each line as a <code>String</code> object.
    /// </summary>
    public class PlainTextByLineStream : ObjectStream<string>
    {
        private readonly FileChannel channel;
        private readonly string encoding;

        private BufferedReader @in;

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="in"> </param>
        public PlainTextByLineStream(Reader @in)
        {
            this.@in = new BufferedReader(@in);
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
            @in = new BufferedReader(Channels.newReader(channel, encoding));
        }

        public PlainTextByLineStream(FileChannel channel, Charset encoding) : this(channel, encoding.name())
        {
        }

        public PlainTextByLineStream(InputStreamReader @in)
        {
            throw new System.NotImplementedException();
        }

        public override string read()
        {
            return @in.readLine();
        }

        public override void reset()
        {
            if (channel == null)
            {
                @in.reset();
            }
            else
            {
                channel.position(0);
                @in = new BufferedReader(Channels.newReader(channel, encoding));
            }
        }

        public virtual void close()
        {
            if (channel == null)
            {
                @in.close();
            }
            else
            {
                channel.close();
            }
        }
    }
}