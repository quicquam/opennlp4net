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
using j4n.IO.InputStream;
using j4n.IO.Reader;

namespace opennlp.model
{
    public class PlainTextFileDataReader : DataReader
    {
        private BufferedReader input;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PlainTextFileDataReader(java.io.File f) throws java.io.IOException
        public PlainTextFileDataReader(Jfile f)
        {
            if (f.Name.EndsWith(".gz", StringComparison.Ordinal))
            {
                input =
                    new BufferedReader(
                        new InputStreamReader(
                            new BufferedInputStream(new GZIPInputStream(new BufferedInputStream(new FileInputStream(f))))));
            }
            else
            {
                input = new BufferedReader(new InputStreamReader(new BufferedInputStream(new FileInputStream(f))));
            }
        }

        public PlainTextFileDataReader(InputStream @in)
        {
            input = new BufferedReader(new InputStreamReader(@in));
        }

        public PlainTextFileDataReader(BufferedReader @in)
        {
            input = @in;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double readDouble() throws java.io.IOException
        public virtual double readDouble()
        {
            return Convert.ToDouble(input.readLine());
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readInt() throws java.io.IOException
        public virtual int readInt()
        {
            return Convert.ToInt32(input.readLine());
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String readUTF() throws java.io.IOException
        public virtual string readUTF()
        {
            return input.readLine();
        }
    }
}