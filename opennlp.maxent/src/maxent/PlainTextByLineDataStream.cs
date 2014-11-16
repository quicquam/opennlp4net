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
using j4n.IO.Reader;

namespace opennlp.maxent
{
    /// <summary>
    /// This DataStream implementation will take care of reading a plain text file
    /// and returning the Strings between each new line character, which is what
    /// many Maxent applications need in order to create EventStreams.
    /// </summary>
    public class PlainTextByLineDataStream : DataStream
    {
        internal BufferedReader dataReader;
        internal string next;

        public PlainTextByLineDataStream(Reader dataSource)
        {
            dataReader = new BufferedReader(dataSource);
            try
            {
                next = dataReader.readLine();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }

        public PlainTextByLineDataStream(StringReader smallReader)
        {
            throw new NotImplementedException();
        }

        public virtual object nextToken()
        {
            string current = next;
            try
            {
                next = dataReader.readLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            return current;
        }

        public virtual bool hasNext()
        {
            return next != null;
        }
    }
}